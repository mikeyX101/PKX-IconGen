""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2026 Samuel Caron/mikeyX#4697

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
import os
import sys
from pathlib import Path

import bpy
from typing import List, Optional, Final, Any

import blender_compat
from data.animation_name import AnimationName
from patcher import apply_patches_by_model_name
from data.edit_mode import EditMode
from data.object_shading import ObjectShading
from data.pokemon_render_data import PokemonRenderData
from data.shiny_color import ColorChannel, ShinyColors, ShinyColor
from data.shiny_info import ShinyInfo
from data.texture import Texture

from importer.shared.helpers.logger import Logger
from importer.importer.importer import Importer

class PkxIconGenCache(object):
    def __init__(self):
        self.shiny_color1: Final[list] = []
        self.shiny_color2: Final[list] = []
        self.shiny_mix: Final[list] = []
        # Mapping/Mat/Defaults by Blender images - (mat_name, mapping_node, [default_x, default_y])
        self.img_mat_mapping: Final[dict[str, list[tuple[str, bpy.types.ShaderNode, list[float]]]]] = {}
        # TexImage by Blender materials
        self.mat_tex_image: Final[dict[str, list]] = {}
        # TexImage by Blender images
        self.img_tex_image: Final[dict[str, list]] = {}
        # Name of materials that have been modified after getting imported
        self.processed_mats: Final[list[str]] = []

    def init_mat_cache(self, mat_name: str):
        self.mat_tex_image[mat_name] = []

    def add_mat_mapping(self, tex_node: bpy.types.ShaderNode, mat: bpy.types.Material):
        if tex_node.bl_idname != "ShaderNodeTexImage":
            print(f"Tried using {tex_node.bl_idname} to add to Mat/Mapping cache")
            return

        if tex_node.image.name not in self.img_mat_mapping:
            self.img_mat_mapping[tex_node.image.name] = []

        mapping_node: bpy.types.ShaderNode = tex_node.inputs[blender_compat.tex_image_in.vector].links[0].from_node
        if mapping_node.bl_idname != "ShaderNodeMapping":
            print(f"Node attached to ShaderNodeTexImage for image {tex_node.image.name} was not ShaderNodeMapping, but {mapping_node.bl_idname} instead.")
        else:
            name: str = mat.name
            x = mapping_node.inputs[1].default_value[0]
            y = mapping_node.inputs[1].default_value[1]
            mat_mapping: tuple[str, Any, list[float]] = (name, mapping_node, [x, y])
            self.img_mat_mapping[tex_node.image.name].append(mat_mapping)


CAMERA_NAME: Final[str] = "PKXIconGen_Camera"
CAMERA_FOCUS_NAME: Final[str] = "PKXIconGen_FocusPoint"

SHINYCOLOR1_NAME: Final[str] = "PKX_ShinyColor1"
SHINYCOLOR2_NAME: Final[str] = "PKX_ShinyColor2"
SHINYMIXNODE_NAME: Final[str] = "PKX_ShinyMixRGB"

cmd_args = None
debugging = False
assets_path: Optional[str] = None
xd_cutout_initial_state: bool = False
do_print_verbose: bool = False
pkx_cache = PkxIconGenCache()

def attach_debugger(debug_egg: str):
    global debugging
    # https://github.com/sybrenstuvel/random-blender-addons/blob/main/remote_debugger.py
    eggpath = os.path.abspath(debug_egg)

    if not os.path.exists(eggpath):
        print(f'Unable to find debug egg at {eggpath}.')
    else:
        if not any('pycharm-debug' in p for p in sys.path):
            sys.path.append(eggpath)

        # noinspection PyUnresolvedReferences
        import pydevd_pycharm
        pydevd_pycharm.settrace('localhost', port=1090, stdout_to_server=True, stderr_to_server=True,
                                suspend=False)
        debugging = True

def parse_cmd_args(script_args):
    global cmd_args
    global assets_path
    global xd_cutout_initial_state
    global do_print_verbose

    if cmd_args is None:
        cmd_args, _ = getopt.getopt(script_args, "", ["pkx-debug=", "debug-egg=", "assets-path=", "xd-cutout", "verbose"])
        for arg, value in cmd_args:
            if arg == "--assets-path" and value != "":
                assets_path = value
                print(f"Assets Path: {assets_path}")
            elif arg == "--xd-cutout":
                xd_cutout_initial_state = True
            elif arg == "--verbose":
                do_print_verbose = True

def print_verbose(msg: str):
    if do_print_verbose:
        print(msg)

def get_absolute_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}", assets_path)


def get_relative_asset_path(model: str) -> str:
    return model.replace("{{AssetsPath}}/", "")

def import_model(model_idx: int, model_path: str):
    objs = bpy.data.objects

    if assets_path is not None:
        true_path = get_absolute_asset_path(model_path)
    else:
        true_path = model_path

    print(f"Importing: {true_path}")
    model_bytes: bytes
    with open(true_path, 'rb') as f:
        model_bytes = f.read()
    options = {
        "ik_hack": True,
        "verbose": False, #do_print_verbose
        "max_frame": 1000000000,
        "section_names": [],
        "filepath": true_path,
        "import_lights": False,
        "import_cameras": False,
        "include_shiny": False,
        "strict_mirror": False
    }
    model_name = Path(true_path).name
    logger = Logger(False, model_name)
    Importer.run(bpy.context, model_bytes, model_name, options, logger)

    armature = objs[bpy.data.armatures[model_idx].name] # While debugging, if an error occurs here, make sure to clear cache between different JSON files
    armature.hide_select = True
    armature.hide_viewport = True


def import_models(prd: PokemonRenderData):
    armatures = bpy.data.armatures
    normal_imported: bool = False
    shiny_imported: bool = False

    shiny_info: ShinyInfo = prd.shiny

    if len(armatures) == 0:
        import_model(0, prd.model)
        normal_imported = True

    if len(armatures) == 1 and shiny_info.model is not None:
        import_model(1, shiny_info.model)

        switch_model(shiny_info, EditMode.FACE_NORMAL)  # Hide shiny model on load
        shiny_imported = True

    if normal_imported or shiny_imported:
        bpy.ops.wm.save_mainfile()
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(os.path.dirname(bpy.data.filepath), "edit.blend"))

    # Remove material animations
    for mat in bpy.data.materials:
        mat.animation_data_clear()

    # Run patches
    #apply_patches_by_model_name(prd.model)
    #apply_patches_by_model_name(prd.shiny.model)

    mats = bpy.data.materials
    for mat in mats:
        if mat.name in pkx_cache.processed_mats:
            break

        tree = mat.node_tree
        if tree is not None:
            #  Reduce bump map strength
            bump_node_idx = tree.nodes.find("Bump")
            if bump_node_idx != -1:
                node_input = tree.nodes[bump_node_idx].inputs[blender_compat.bump_in.strength]
                if node_input.default_value == 1:  # Patches could change the value, changes only if it's still the same
                    node_input.default_value = 0.05

            # Setup shiny color
            if shiny_info.color1 is not None and shiny_info.color2 is not None and tree.nodes.find(f"{SHINYMIXNODE_NAME}.0") == -1:
                setup_shiny_mats(tree)

            # Cache
            pkx_cache.processed_mats.append(mat.name)

            pkx_cache.init_mat_cache(mat.name)
            for node in tree.nodes:
                if node.bl_idname == "ShaderNodeTexImage":
                    pkx_cache.mat_tex_image[mat.name].append(node)
                    pkx_cache.add_mat_mapping(node, mat)
                elif SHINYCOLOR1_NAME in node.name:
                    pkx_cache.shiny_color1.append(node)
                elif SHINYCOLOR2_NAME in node.name:
                    pkx_cache.shiny_color2.append(node)
                elif SHINYMIXNODE_NAME in node.name:
                    pkx_cache.shiny_mix.append(node)

    if shiny_info.color1 is not None and shiny_info.color2 is not None:
        update_all_shiny_colors(shiny_info.color1, shiny_info.color2)


def setup_shiny_mats(tree: bpy.types.NodeTree):
    uses_bump: bool = tree.nodes.find("Bump") != -1
    principled_bsdf = get_principled_bsdf_from_tree_nodes(tree)
    principled_bsdf_in_links = principled_bsdf.inputs[blender_compat.principled_bsdf_in.base_color].links
    uses_single_color: bool = principled_bsdf_in_links[0].from_node.bl_idname == "ShaderNodeRGB" if len(principled_bsdf_in_links) > 0 else False

    if uses_single_color:
        rgb_node = principled_bsdf.inputs[blender_compat.principled_bsdf_in.base_color].links[0].from_node

        color2_node = tree.nodes.new("ShaderNodeGroup")
        color2_node.name = f"{SHINYCOLOR2_NAME}.color"
        color2_node.node_tree = bpy.data.node_groups['Color2']

        mix_node = tree.nodes.new("ShaderNodeMixRGB")
        mix_node.name = f"{SHINYMIXNODE_NAME}.color"
        mix_node.blend_type = "MIX"
        mix_node.inputs[0].default_value = 0

        tree.links.new(rgb_node.outputs[0], color2_node.inputs[0])  # ShaderNodeRGB.Color -> PKX_ShinyColor2.Color
        tree.links.new(color2_node.outputs[0], mix_node.inputs[blender_compat.mix_in.color2])  # PKX_ShinyColor2.Color -> PKX_ShinyMixRGB.Color2
        tree.links.new(rgb_node.outputs[0], mix_node.inputs[blender_compat.mix_in.color1])  # ShaderNodeRGB.Color -> PKX_ShinyMixRGB.Color1
        tree.links.new(mix_node.outputs[0], principled_bsdf.inputs[blender_compat.principled_bsdf_in.base_color])  # PKX_ShinyMixRGB.Color -> PrincipledBSDF.Color
    else:
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

                tree.links.new(color2_node.outputs[0], mix_node.inputs[blender_compat.mix_in.color2])  # PKX_ShinyColor2.TexColor -> PKX_ShinyMixRGB.Color2
                # TODO Manage Alpha

                tree.links.new(node.outputs[0], mix_node.inputs[blender_compat.mix_in.color1])  # ShaderNodeTexImage.Color -> PKX_ShinyMixRGB.Color1
                tree.links.new(mix_node.outputs[0], texture_output_socket)  # PKX_ShinyMixRGB.Color -> Previous output

                tex_number += 1


def get_principled_bsdf_from_tree_nodes(tree):
    for node in tree.nodes:
        if node.bl_idname == "ShaderNodeBsdfPrincipled":
            return node
    raise Exception("No Principled BSDF found")

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
        if mode in EditMode.ANY_SHINY:
            show_shiny_mats()
        else:
            hide_shiny_mats()
    elif shiny_info.model is not None:
        objs = bpy.data.objects

        show_shiny = mode in EditMode.ANY_SHINY
        show_armature(objs[bpy.data.armatures[0].name], not show_shiny)
        show_armature(objs[bpy.data.armatures[1].name], show_shiny)
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
        if bpy.data.images.find(texture.name) == -1:
            print("Image not found, ignoring. [%s]" % texture.name)
            if texture.path is not None:
                full_path: str = get_absolute_asset_path(texture.path)
                unlinked_img = bpy.data.images.load(filepath=full_path, check_existing=True)
                unlinked_img.name = "UNLINKED_" + texture.name
            continue

        custom_img = None
        if texture.path is not None:
            full_path: str = get_absolute_asset_path(texture.path)
            set_custom_image(bpy.data.images[texture.name], full_path)
            custom_img = bpy.data.images.load(filepath=full_path, check_existing=True)

        original_img = bpy.data.images[texture.name]
        for mat in texture.mats:
            set_material_map(custom_img or original_img,
                             bpy.data.materials[mat.name],
                             mat.map.x,
                             mat.map.y)


def reset_texture_images(texture: Texture):
    if texture.path is not None:
        custom_img = bpy.data.images.load(filepath=get_absolute_asset_path(texture.path), check_existing=True)
        if bpy.data.images.find(texture.name) == -1:
            print("Image not found, ignoring. [%s]" % texture.name)
            custom_img.name = "UNLINKED_" + texture.name
            return

        original_img = bpy.data.images[texture.name]

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


def reset_materials_maps():
    for img_infos in list(pkx_cache.img_mat_mapping.values()):
        for (_, mapping_node, defaults) in img_infos:
            mapping_node.inputs[1].default_value[0] = defaults[0]
            mapping_node.inputs[1].default_value[1] = defaults[1]


def get_armature_obj(prd: PokemonRenderData, mode: EditMode):
    shiny_model = prd.shiny.model
    if shiny_model is not None and shiny_model != "" and mode in EditMode.ANY_SHINY:
        armature = bpy.data.armatures[1]
    else:
        armature = bpy.data.armatures[0]

    return bpy.data.objects[armature.name]


def show_armature(armature_obj, show: bool):
    hide = not show

    armature_obj.hide_render = hide

    for child in armature_obj.children:
        if not child.hide_get(): # Ignore importer hidden meshes
            child.hide_render = hide
            child.hide_viewport = hide


def allow_select_all_armatures(allow_select: bool):
    not_allow_select = not allow_select

    objs = bpy.data.objects
    for child in objs[bpy.data.armatures[0].name].children:
        child.hide_select = not_allow_select

    shiny_armature_idx = objs.find(bpy.data.armatures[1].name) if len(bpy.data.armatures) > 1 else -1
    if shiny_armature_idx != -1:
        for child in objs[shiny_armature_idx].children:
            child.hide_select = not_allow_select


def remove_objects(removed_objects: List[str]):
    objs = bpy.data.objects
    for obj_name in removed_objects:
        if objs.find(obj_name) == -1:
            print("Removed object not found, ignoring. [%s]" % obj_name)
            continue
        obj = objs[obj_name]
        obj.hide_render = True
        obj.hide_viewport = True


def update_shading(shading: ObjectShading):
    use_smooth = shading == ObjectShading.SMOOTH

    objs = bpy.data.objects
    for child in objs[bpy.data.armatures[0].name].children:
        for polygon in child.data.polygons:
            polygon.use_smooth = use_smooth

    shiny_armature_idx = objs.find(bpy.data.armatures[1].name) if len(bpy.data.armatures) > 1 else -1
    if shiny_armature_idx != -1:
        for child in objs[shiny_armature_idx].children:
            for polygon in child.data.polygons:
                polygon.use_smooth = use_smooth


def get_animation_action(model: str, animation_name: AnimationName):
    suffix = "Idle"
    if animation_name == AnimationName.PHYSICAL_ATTACK:
        suffix = "Physical"
    elif animation_name == AnimationName.SPECIAL_ATTACK:
        suffix = "Attack"
    elif animation_name == AnimationName.TAKING_DAMAGE:
        suffix = "Damage"
    elif animation_name == AnimationName.FAINTING:
        suffix = "Faint"

    clean_model_name = Path(get_relative_asset_path(model)).stem
    actions = bpy.data.actions.values()
    for action in actions:
        name = action.name
        if name.startswith(clean_model_name) and name.endswith(suffix):
            return action

    raise Exception(f"Animation not found for model {clean_model_name}, animation {animation_name}!{' Debugging is enabled, clear debug cache first?' if debugging else ''}")


def get_view3d_context():
    context_override = bpy.context.copy()
    context_override["screen"] = bpy.data.screens["Layout.001"]
    context_override["area"] = context_override["screen"].areas[0]
    for region in context_override["area"].regions:
        if region.type == "WINDOW":
            context_override["region"] = region
            break

    return context_override


def focus_view3d_on_objects(objects: List[bpy.types.Object]):
    with bpy.context.temp_override(**get_view3d_context()):
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objects:
            obj.select_set(True)
        bpy.ops.view3d.view_selected()
        bpy.ops.object.select_all(action='DESELECT')


# for active scene camera
def focus_camera_on_objects(objects: List[bpy.types.Object]):
    with bpy.context.temp_override(**get_view3d_context()):
        bpy.ops.object.select_all(action='DESELECT')
        for obj in objects:
            obj.select_set(True)
        bpy.ops.view3d.camera_to_view_selected()
        bpy.ops.object.select_all(action='DESELECT')
