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
import os
import sys

import bpy
from typing import List, Optional, Final

import blender_compat
from data.edit_mode import EditMode
from data.object_shading import ObjectShading
from data.pokemon_render_data import PokemonRenderData
from data.shiny_color import ColorChannel, ShinyColors, ShinyColor
from data.shiny_info import ShinyInfo
from data.texture import Texture
from importer import import_hsd


class PkxIconGenCache(object):
    def __init__(self):
        self.shiny_color1: list = []
        self.shiny_color2: list = []
        self.shiny_mix: list = []
        # Mapping by Blender materials
        self.mapping: dict[str, list] = {}
        # TexImage by Blender materials
        self.mat_tex_image: dict[str, list] = {}
        # TexImage by Blender images
        self.img_tex_image: dict[str, list] = {}

    def init_mat_cache(self, mat_name: str):
        self.mapping[mat_name] = []
        self.mat_tex_image[mat_name] = []


SHINYCOLOR1_NAME: Final[str] = "PKX_ShinyColor1"
SHINYCOLOR2_NAME: Final[str] = "PKX_ShinyColor2"
SHINYMIXNODE_NAME: Final[str] = "PKX_ShinyMixRGB"

cmd_args = None
assets_path: Optional[str] = None
pkx_cache = PkxIconGenCache()


def attach_debugger(debug_egg: str):
    # https://github.com/sybrenstuvel/random-blender-addons/blob/main/remote_debugger.py
    eggpath = os.path.abspath(debug_egg)

    if not os.path.exists(eggpath):
        print(f'Unable to find debug egg at {eggpath}.')
    else:
        if not any('pycharm-debug' in p for p in sys.path):
            sys.path.append(eggpath)

        import pydevd_pycharm
        pydevd_pycharm.settrace('localhost', port=1090, stdoutToServer=True, stderrToServer=True,
                                suspend=False)


def parse_cmd_args(script_args):
    global cmd_args
    global assets_path

    if cmd_args is None:
        cmd_args, _ = getopt.getopt(script_args, "", ["pkx-debug=", "debug-egg=", "assets-path="])
        for arg, value in cmd_args:
            if arg == "--assets-path" and value != "":
                assets_path = value
                print(f"Assets Path: {assets_path}")


def get_absolute_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}", assets_path)


def get_relative_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}/", "")


def import_models(prd: PokemonRenderData):
    objs = bpy.data.objects
    any_imported: bool = False

    shiny_info: ShinyInfo = prd.shiny

    if objs.find("Armature0") == -1:
        if assets_path is not None:
            true_path = get_absolute_asset_path(prd.render.model)
        else:
            true_path = prd.render.model

        print(f"Importing: {true_path}")
        import_hsd.load(None, bpy.context, true_path, 0, "scene_data", "SCENE", True, True, 1000, True)
        any_imported = True

    if objs.find("Armature1") == -1 and shiny_info.render.model is not None:
        if assets_path is not None:
            shiny_true_path = get_absolute_asset_path(shiny_info.render.model)
        else:
            shiny_true_path = shiny_info.render.model

        print(f"Importing: {shiny_true_path}")
        import_hsd.load(None, bpy.context, shiny_true_path, 0, "scene_data", "SCENE", True, True, 1000, True)
        switch_model(shiny_info, EditMode.NORMAL)  # Hide shiny model on load
        any_imported = True

    if any_imported:
        bpy.ops.wm.save_mainfile()
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(os.path.dirname(bpy.data.filepath), "edit.blend"))

    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            #  Rough Materials
            # TODO Vertex Lighting
            bsdf = tree.nodes["Principled BSDF"]
            bsdf.inputs[blender_compat.principled_bsdf_in.metallic].default_value = 0
            bsdf.inputs[blender_compat.principled_bsdf_in.specular].default_value = 0
            bsdf.inputs[blender_compat.principled_bsdf_in.roughness].default_value = 1

            #  Reduce bump map strength
            if tree.nodes.find("Bump") != -1:
                tree.nodes["Bump"].inputs[blender_compat.bump_in.strength].default_value = 0.05

            #  Fix normal maps output being in Alpha
            alpha_input = bsdf.inputs[blender_compat.principled_bsdf_in.alpha]
            if len(alpha_input.links) > 0:
                bump_link = alpha_input.links[0]
                bump_node = bump_link.from_node
                if bump_node.bl_idname == "ShaderNodeBump":
                    tree.links.remove(bump_link)
                    tree.links.new(bump_node.outputs[0], bsdf.inputs[blender_compat.principled_bsdf_in.normal])

            #  Fix alpha output being in Emission Strength/Transmission Roughness
            transmission_roughness_input = bsdf.inputs[blender_compat.principled_bsdf_in.transmission_roughness]
            if len(transmission_roughness_input.links) > 0:
                alpha_link = transmission_roughness_input.links[0]
                alpha_node = alpha_link.from_node
                tree.links.remove(alpha_link)
                tree.links.new(alpha_node.outputs[0], bsdf.inputs[blender_compat.principled_bsdf_in.alpha])

            emission_strength_input = bsdf.inputs[blender_compat.principled_bsdf_in.emission_strength]
            if len(emission_strength_input.links) > 0:
                alpha_link = emission_strength_input.links[0]
                alpha_node = alpha_link.from_node
                tree.links.remove(alpha_link)
                tree.links.new(alpha_node.outputs[0], bsdf.inputs[blender_compat.principled_bsdf_in.alpha])

            # Setup shiny color
            if shiny_info.color1 is not None and shiny_info.color2 is not None and tree.nodes.find(f"{SHINYMIXNODE_NAME}.0") == -1:
                setup_shiny_mats(tree)

            # Cache nodes
            pkx_cache.init_mat_cache(mat.name)
            for node in tree.nodes:
                if node.bl_idname == "ShaderNodeMapping":
                    pkx_cache.mapping[mat.name].append(node)
                elif node.bl_idname == "ShaderNodeTexImage":
                    pkx_cache.mat_tex_image[mat.name].append(node)
                elif SHINYCOLOR1_NAME in node.name:
                    pkx_cache.shiny_color1.append(node)
                elif SHINYCOLOR2_NAME in node.name:
                    pkx_cache.shiny_color2.append(node)
                elif SHINYMIXNODE_NAME in node.name:
                    pkx_cache.shiny_mix.append(node)

    if shiny_info.color1 is not None and shiny_info.color2 is not None:
        update_all_shiny_colors(shiny_info.color1, shiny_info.color2)


def setup_shiny_mats(tree):
    uses_bump: bool = tree.nodes.find("Bump") != -1

    tex_number: int = 0
    for node in tree.nodes:
        if node.bl_idname == "ShaderNodeTexImage":
            texture_output_socket = node.outputs[0].links[0].to_socket

            if not uses_bump:
                color1_node = tree.nodes.new("ShaderNodeGroup")
                color1_node.name = f"{SHINYCOLOR1_NAME}.{tex_number}"
                color1_node.node_tree = bpy.data.node_groups['Color1']
            else:
                color1_node = None

            color2_node = tree.nodes.new("ShaderNodeGroup")
            color2_node.name = f"{SHINYCOLOR2_NAME}.{tex_number}"
            color2_node.node_tree = bpy.data.node_groups['Color2']

            mix_node = tree.nodes.new("ShaderNodeMixRGB")
            mix_node.name = f"{SHINYMIXNODE_NAME}.{tex_number}"
            mix_node.blend_type = "MIX"
            mix_node.inputs[0].default_value = 0

            if color1_node is not None:
                tree.links.new(node.outputs[0], color1_node.inputs[0])  # ShaderNodeTexImage.Color -> PKX_ShinyColor1.TexColor
                tree.links.new(node.outputs[1], color1_node.inputs[1])  # ShaderNodeTexImage.Alpha -> PKX_ShinyColor1.TexAlpha

                tree.links.new(color1_node.outputs[0], color2_node.inputs[0])  # PKX_ShinyColor1.TexColor -> PKX_ShinyColor2.Image
                tree.links.new(color1_node.outputs[1], color2_node.inputs[1])  # PKX_ShinyColor1.TexAlpha -> PKX_ShinyColor2.Alpha
            else:
                tree.links.new(node.outputs[0], color2_node.inputs[0])  # ShaderNodeTexImage.Color -> PKX_ShinyColor2.Image
                tree.links.new(node.outputs[1], color2_node.inputs[1])  # ShaderNodeTexImage.Alpha -> PKX_ShinyColor2.Alpha

            tree.links.new(color2_node.outputs[0], mix_node.inputs[2])  # PKX_ShinyColor2.TexColor -> PKX_ShinyMixRGB.Color2
            # TODO Manage Alpha

            tree.links.new(node.outputs[0], mix_node.inputs[1])  # ShaderNodeTexImage.Color -> PKX_ShinyMixRGB.Color1
            tree.links.new(mix_node.outputs[0], texture_output_socket)  # PKX_ShinyMixRGB.Color -> Previous output

            tex_number += 1


def show_shiny_mats():
    for node in pkx_cache.shiny_mix:
        node.inputs[0].default_value = 1


def hide_shiny_mats():
    for node in pkx_cache.shiny_mix:
        node.inputs[0].default_value = 0


def update_shiny_color(color: int, channel: ColorChannel, shiny_color: ShinyColors):
    if shiny_color == ShinyColors.Color1:
        for node in pkx_cache.shiny_color1:
            node.inputs[channel].default_value = color
    elif shiny_color == ShinyColors.Color2:
        for node in pkx_cache.shiny_color2:
            node.inputs[channel].default_value = color


def update_all_shiny_colors(color1: ShinyColor, color2: ShinyColor):
    for node in pkx_cache.shiny_color1:
        node.inputs[ColorChannel.R].default_value = color1.r
        node.inputs[ColorChannel.G].default_value = color1.g
        node.inputs[ColorChannel.B].default_value = color1.b
        node.inputs[ColorChannel.A].default_value = color1.a
    for node in pkx_cache.shiny_color2:
        node.inputs[ColorChannel.R].default_value = color2.r
        node.inputs[ColorChannel.G].default_value = color2.g
        node.inputs[ColorChannel.B].default_value = color2.b
        node.inputs[ColorChannel.A].default_value = color2.a


def show_message_box(message, title, icon='INFO'):
    def draw(self, context):
        self.layout.label(text=message)

    bpy.context.window_manager.popup_menu(draw, title=title, icon=icon)


def switch_model(shiny_info: ShinyInfo, mode: EditMode):
    if shiny_info.color1 is not None and shiny_info.color2 is not None:
        if mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            show_shiny_mats()
        else:
            hide_shiny_mats()
    elif shiny_info.render.model is not None:
        objs = bpy.data.objects

        if mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            hide_armature(objs["Armature0"])
            show_armature(objs["Armature1"])
        else:
            hide_armature(objs["Armature1"])
            show_armature(objs["Armature0"])
    else:
        raise Exception("PRD had no filter or alt model.")


def get_image_nodes(image_obj):
    if image_obj.name not in pkx_cache.img_tex_image:
        nodes: list = list()
        if image_obj is not None:
            for mat in bpy.data.materials:
                if mat.node_tree is not None:
                    for node in mat.node_tree.nodes:
                        if is_node_teximage_with_image(node, image_obj):
                            nodes.append(node)
        pkx_cache.img_tex_image[image_obj.name] = nodes

    return pkx_cache.img_tex_image[image_obj.name]


def get_image_materials(image_obj):
    mats: list = list()
    if image_obj is not None:
        for mat in bpy.data.materials:
            if mat.node_tree is not None:
                for node in mat.node_tree.nodes:
                    if is_node_teximage_with_image(node, image_obj):
                        mats.append(mat)
                        break
    return mats


def set_custom_image(image_obj, texture_path: str) -> bool:
    success: bool = False

    nodes = get_image_nodes(image_obj)
    new_img = bpy.data.images.load(filepath=texture_path, check_existing=True)
    if image_is_integer_scale(image_obj, new_img):
        success = True
        for node in nodes:
            node.image = new_img
    return success


def image_is_integer_scale(original_img, new_img) -> bool:
    o_width: int = original_img.size[0]
    o_height: int = original_img.size[1]
    n_width: int = new_img.size[0]
    n_height: int = new_img.size[1]

    width_scale_check: bool = n_width % o_width == 0
    height_scale_check: bool = n_height % o_height == 0
    ratio_check: bool = o_width / o_height == n_width / n_height

    return width_scale_check and height_scale_check and ratio_check


def set_textures(textures: List[Texture]):
    for texture in textures:
        original_img = bpy.data.images[texture.name]
        custom_img = None
        if texture.path is not None:
            full_path: str = get_absolute_asset_path(texture.path)
            set_custom_image(bpy.data.images[texture.name], full_path)
            custom_img = bpy.data.images.load(filepath=full_path, check_existing=True)

        for mat in texture.mats:
            set_material_map(custom_img or original_img,
                             bpy.data.materials[mat.name],
                             mat.map.x,
                             mat.map.y)


def reset_texture_images(texture: Texture):
    if texture.path is not None:
        original_img = bpy.data.images[texture.name]
        custom_img = bpy.data.images.load(filepath=get_absolute_asset_path(texture.path), check_existing=True)

        nodes = get_image_nodes(custom_img)

        for node in nodes:
            node.image = original_img


def is_node_teximage_with_image(node, image_obj) -> bool:
    return node.bl_idname == "ShaderNodeTexImage" and node.image.name == image_obj.name


def is_custom_texture_used(texture_path: str, textures: List[Texture]):
    for texture in textures:
        if texture.path is not None and get_absolute_asset_path(texture.path) == texture_path:
            return True

    return False


def set_material_map(image_obj, mat_obj, x: float, y: float):
    for node in pkx_cache.mat_tex_image[mat_obj.name]:
        if is_node_teximage_with_image(node, image_obj):
            img_vector_input_node = node.inputs[blender_compat.tex_image_in.vector].links[0].from_node
            if img_vector_input_node.bl_idname == "ShaderNodeMapping":
                img_vector_input_node.inputs[1].default_value[0] = x
                img_vector_input_node.inputs[1].default_value[1] = y


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


def update_shading(shading: ObjectShading, context=None):
    context = context or bpy.context
    current_selected = list(context.selected_objects)
    bpy.ops.object.select_all(action="SELECT")

    if shading == ObjectShading.FLAT:
        bpy.ops.object.shade_flat()
    elif shading == ObjectShading.SMOOTH:
        bpy.ops.object.shade_smooth()

    bpy.ops.object.select_all(action="DESELECT")
    for obj in current_selected:
        obj.select_set(True)


def hue2rgb(hue: float) -> List[float]:
    """Input hue is [0, 1]"""
    sat = 1
    val = 1
    hue = convert_range(0, 1, 0, 360, hue)

    c: float = sat * val
    m: float = val - c
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
        rgb[i] += m
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
