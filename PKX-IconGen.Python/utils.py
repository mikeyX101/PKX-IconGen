""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
"""

import getopt
import math
import bpy
from typing import List, Optional, Final

from data.color import Color
from data.edit_mode import EditMode
from data.pokemon_render_data import PokemonRenderData
from data.shiny_info import ShinyInfo
from data.texture import Texture
from importer import import_hsd

RGBNODE_NAME: Final[str] = "PKX_ShinyRGB"
MIXNODE_NAME: Final[str] = "PKX_MixRGB"

cmd_args = None
assets_path: Optional[str] = None


def parse_cmd_args(script_args):
    global cmd_args
    global assets_path

    if cmd_args is None:
        cmd_args, _ = getopt.getopt(script_args, "", ["pkx-debug=", "assets-path="])
        for arg, value in cmd_args:
            if arg == "--assets-path" and value != "":
                assets_path = value


def get_absolute_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}", assets_path)


def get_relative_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}/", "")


def import_model(model: str, shiny_hue: Optional[Color]):
    print(f"Assets Path: {assets_path}")
    if assets_path is not None:
        true_path = get_absolute_asset_path(model)
    else:
        true_path = model

    print(f"Importing: {true_path}")
    import_hsd.load(None, bpy.context, true_path, 0, "scene_data", "SCENE", True, True, 1000, True)

    blender_ver = bpy.app.version
    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            #  Rough Materials
            #  TODO Vertex Lighting
            if (2, 93, 0) <= blender_ver < (2, 94, 0):
                bsdf = tree.nodes["Principled BSDF"]
                bsdf.inputs[4].default_value = 0  # Metallic
                bsdf.inputs[5].default_value = 0  # Specular
                bsdf.inputs[7].default_value = 1  # Roughness
            elif (3, 0, 0) <= blender_ver < (3, 1, 0):  # TODO Validate nodes in 3.1 and 3.2
                bsdf = tree.nodes["Principled BSDF"]
                bsdf.inputs[6].default_value = 0  # Metallic
                bsdf.inputs[7].default_value = 0  # Specular
                bsdf.inputs[9].default_value = 1  # Roughness

            if shiny_hue is not None and tree.nodes.find(MIXNODE_NAME) == -1:
                mix_node = tree.nodes.new("ShaderNodeMixRGB")
                mix_node.name = MIXNODE_NAME
                mix_node.blend_type = "HUE"

                color_data_node = tree.nodes["Principled BSDF"].inputs[0].links[0].from_node  # Base Color
                tree.links.remove(tree.nodes["Principled BSDF"].inputs[0].links[0])

                rgb_node = tree.nodes.new("ShaderNodeRGB")
                rgb_node.name = RGBNODE_NAME
                tree.links.new(rgb_node.outputs[0], mix_node.inputs[2])

                rgb = hue2rgb(shiny_hue)
                rgb.append(0)
                rgb_node.outputs[0].default_value = rgb
                tree.links.new(color_data_node.outputs[0], mix_node.inputs[1])
                tree.links.new(mix_node.outputs[0], tree.nodes["Principled BSDF"].inputs[0])
                mix_node.inputs[0].default_value = 0


def show_shiny_mats():
    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            if tree.nodes.find("PKX_MixRGB") != -1:
                mix_node = tree.nodes["PKX_MixRGB"]
                mix_node.inputs[0].default_value = 1
            else:
                raise Exception("Shiny material was not setup for material: " + mat.name)


def hide_shiny_mats():
    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            if tree.nodes.find("PKX_MixRGB") != -1:
                mix_node = tree.nodes["PKX_MixRGB"]
                mix_node.inputs[0].default_value = 0
            else:
                raise Exception("Shiny material was not setup for material: " + mat.name)


def change_shiny_color(rgb: List[float]):
    rgba: List[float] = []
    rgba.extend(rgb)
    rgba.append(1)

    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            tree.nodes[RGBNODE_NAME].outputs[0].default_value = rgba


def show_message_box(message, title, icon='INFO'):

    def draw(self, context):
        self.layout.label(text=message)

    bpy.context.window_manager.popup_menu(draw, title=title, icon=icon)


def switch_model(shiny_info: ShinyInfo, mode: EditMode):
    if shiny_info.hue is not None:
        if mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            show_shiny_mats()
        else:
            hide_shiny_mats()
    elif shiny_info.render.model is not None:
        objs = bpy.data.objects

        if mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            if objs.find("Armature1") == -1:
                # show_message_box("Loading shiny model. Please wait...", "Loading...")
                import_model(shiny_info.render.model, None)

            hide_armature(objs["Armature0"])
            show_armature(objs["Armature1"])
        else:
            if objs.find("Armature1") != -1:
                hide_armature(objs["Armature1"])

            show_armature(objs["Armature0"])
    else:
        raise Exception("PRD had no filter or alt model.")


def get_image_nodes(image_obj):
    nodes: list = list()
    if image_obj is not None:
        for mat in bpy.data.Materials:
            node_tree = mat.node_tree
            for node in node_tree.nodes:
                if is_node_teximage_with_image(node, image_obj):
                    nodes.append(node)
                    break
    return nodes


def set_custom_image(image_obj, texture_path: str):
    nodes = get_image_nodes(image_obj)
    new_image = bpy.data.images.load(filepath=texture_path, check_existing=True)
    for node in nodes:
        node.image = new_image


def reset_texture_images(texture: Texture):
    original_image = bpy.data.images[texture.texture_name]
    custom_image = bpy.data.images.load(filepath=texture.image_path, check_existing=True)

    nodes = get_image_nodes(custom_image)

    for node in nodes:
        node.image = new_image


def is_node_teximage_with_image(node, image_obj):
    return node.bl_idname == "ShaderNodeTexImage" and node.image.name == image_obj.name


def set_material_map(mat_obj, x: float, y: float):

    pass


def get_armature(prd: PokemonRenderData, mode: EditMode):
    objs = bpy.data.objects
    shiny_model = prd.shiny.render.model
    if shiny_model is not None and shiny_model != "" and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY):
        armature = objs["Armature1"]
    else:
        armature = objs["Armature0"]

    return armature


def show_armature(armature_obj):
    armature_obj.hide_render = False
    armature_obj.hide_viewport = False

    for child in armature_obj.children:
        child.hide_render = False
        child.hide_viewport = False


def hide_armature(armature_obj):
    armature_obj.hide_render = True
    armature_obj.hide_viewport = True

    for child in armature_obj.children:
        child.hide_render = True
        child.hide_viewport = True


def remove_objects(removed_objects: List[str]):
    objs = bpy.data.objects

    for obj_name in removed_objects:
        obj = objs[obj_name]
        obj.hide_render = True
        obj.hide_viewport = True


def hue2rgb(hue: float) -> List[float]:
    """Input hue is [0, 1]"""
    sat = 1
    val = 1
    hue = convert_range(0, 1, 0, 360, hue)

    c: float = sat * val
    m: float = 0
    h: float = hue / 60
    x: float = c * (1 - math.fabs(h % 2 - 1))

    rgb: List[float] = []
    if 0 <= h < 1:
        rgb = [c, x, 0]
    elif 1 <= h < 2:
        rgb = [x, c, 0]
    elif 2 <= h < 3:
        rgb = [0, c, x]
    elif 3 <= h < 4:
        rgb = [0, x, c]
    elif 4 <= h < 5:
        rgb = [x, 0, c]
    elif 5 <= h <= 6:
        rgb = [c, 0, x]

    for i in range(len(rgb)):
        rgb[i] = round(rgb[i] + m)
    return rgb


def rgb2hue(rgb: List[float]) -> float:
    """Result hue is [0, 1]"""
    r: float = rgb[0]
    g: float = rgb[1]
    b: float = rgb[2]

    max_c: float = max(rgb)
    min_c: float = min(rgb)
    delta: float = max_c - min_c

    hue: float = 0
    if delta == 0:
        hue = 0
    elif max_c == r:
        hue = ((g - b) / delta) % 6
    elif max_c == g:
        hue = ((b - r) / delta) + 2
    elif max_c == b:
        hue = ((r - g) / delta) + 4

    return convert_range(0, 6, 0, 1, hue)


def convert_range(original_start: float, original_end: float, new_start: float, new_end: float, value: float) -> float:
    if original_start > value:
        raise ValueError("Value was smaller than the original range.")

    elif original_end < value:
        raise ValueError("Value was greater than the original range.")

    scale: float = (new_end - new_start) / (original_end - original_start)
    return new_start + (value - original_start) * scale
