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
import copy
from typing import List, Optional
import bpy
import os

import bpy_extras.view3d_utils

import common
from data.color import Color
from data.data_type import DataType
from data.light import Light, LightType
from data.material import Material
from data.object_shading import ObjectShading
from data.pokemon_render_data import PokemonRenderData
from data.render_data import RenderData
from data.render_target import RenderTarget
from data.camera import Camera
from data.shiny_color import ColorChannel, ShinyColors, ShinyColor
from data.texture import Texture
from data.vector2 import Vector2
from data.vector3 import Vector3
from data.edit_mode import EditMode
from math import degrees, radians

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "version": (0, 3, 12),
    "category": "User Interface",
    "description": "Addon to help users use PKX-IconGen without any Blender knowledge",
    "author": "Samuel Caron/mikeyx",
    "location": "View3D > PKX-IconGen",
    "doc_url": "https://github.com/mikeyX101/PKX-IconGen"
}
addon_ver_str: str = '.'.join([str(v) for v in bl_info['version']])

addon_keymaps = []

mode: EditMode = EditMode.FACE_NORMAL  # Mode should be a singular flag
prd: PokemonRenderData
removed_objects: List[str] = list()
render_textures: dict[str, Texture] = dict()
custom_texture_path_invalid: bool = False
custom_texture_scale_invalid: bool = False
custom_texture_reused: bool = False


# Operators
class ShowRegionUiOperator(bpy.types.Operator):
    """Show region UI"""
    bl_idname = "wm.show_region_ui"
    bl_label = "Show region UI"

    @classmethod
    def poll(cls, context):
        return context.active_object is not None

    def execute(self, context):
        bpy.ops.wm.context_toggle(data_path="space_data.show_region_ui")
        return {'FINISHED'}


class PKXReplaceByAssetsPathOperator(bpy.types.Operator):
    """Replaces the full AssetsPath with {{AssetsPath}}, path and image needs to be valid. If path is empty, adds the full AssetsPath"""
    bl_idname = "wm.pkx_assets_path"
    bl_label = "{{AssetsPath}}"

    @classmethod
    def poll(cls, context):
        return len(context.scene.custom_texture_path) == 0 or (not custom_texture_path_invalid and not custom_texture_scale_invalid and not custom_texture_reused)

    def execute(self, context):
        if len(context.scene.custom_texture_path) == 0:
            context.scene.custom_texture_path = common.assets_path + "/icon-gen/"
        else:
            context.scene.custom_texture_path = context.scene.custom_texture_path.replace(common.assets_path, "{{AssetsPath}}")
        return {'FINISHED'}


class PKXDeleteOperator(bpy.types.Operator):
    """Delete selected items, useful for getting rid of duplicate meshes or bounding box cubes"""
    bl_idname = "wm.pkx_delete"
    bl_label = "Delete selected items"

    @classmethod
    def poll(cls, context):
        return len([obj for obj in context.selected_objects if "PKXIconGen_" not in obj.name]) > 0

    def execute(self, context):
        objs = context.selected_objects

        objects_to_remove = [obj for obj in objs if "PKXIconGen_" not in obj.name]
        removed_objects.extend([obj.name for obj in objects_to_remove])
        for obj in objects_to_remove:
            obj.hide_render = True
            obj.hide_viewport = True

        return {'FINISHED'}


class PKXResetDeletedOperator(bpy.types.Operator):
    """Restores every deleted item"""
    bl_idname = "wm.pkx_reset_deleted"
    bl_label = "Reset deleted items"

    @classmethod
    def poll(cls, context):
        return len(removed_objects) > 0

    def execute(self, context):
        global removed_objects
        removed_objects = []
        common.show_armature(get_armature(), True)

        return {'FINISHED'}


class PKXCopyToOperator(bpy.types.Operator):
    """Copy selected data from the current mode to another mode"""
    bl_idname = "wm.pkx_copy_to"
    bl_label = "Copy to"

    @classmethod
    def poll(cls, context):
        return context.scene.copy_mode != "" and len(context.scene.items_to_copy) != 0

    def execute(self, context):
        scene = context.scene

        if scene.advanced_camera_editing:
            sync_camera_to_props(context)
        sync_props_to_prd(context)

        items_to_copy: set[str] = scene.items_to_copy
        copy_from: EditMode = get_edit_mode(scene)
        copy_to: EditMode = EditMode[scene.copy_mode]

        copy_prd_data(copy_from, copy_to, items_to_copy)

        return {'FINISHED'}


class PKXCopyFromOperator(bpy.types.Operator):
    """Copy selected data from another mode to the current mode"""
    bl_idname = "wm.pkx_copy_from"
    bl_label = "Copy from"

    @classmethod
    def poll(cls, context):
        return context.scene.copy_mode != "" and len(context.scene.items_to_copy) != 0

    def execute(self, context):
        scene = context.scene

        if scene.advanced_camera_editing:
            sync_camera_to_props(context)
        sync_props_to_prd(context)

        items_to_copy: set[str] = scene.items_to_copy
        copy_from: EditMode = EditMode[scene.copy_mode]
        copy_to: EditMode = get_edit_mode(scene)

        copy_prd_data(copy_from, copy_to, items_to_copy)

        # Current mode gets changed, so we need to sync up the props and the scene
        sync_prd_to_props(context)
        sync_props_to_scene(context)

        return {'FINISHED'}


def copy_prd_data(copy_from: EditMode, copy_to: EditMode, items_to_copy: set[str]):
    data_flags = DataType.from_blender_flags(items_to_copy)
    for data_type in DataType:
        data_type_name: str = data_type.name
        allowed = is_data_type_allowed_copy(data_type_name, copy_from, copy_to)
        if not allowed and data_type in data_flags:
            data_flags ^= data_type
            print(f"Copy of {data_type_name} is not allowed in this context ({copy_from} -> {copy_to}), ignoring data type.")

    source: RenderData = prd.get_mode_render(copy_from)
    target: RenderData = prd.get_mode_render(copy_to)

    if DataType.ANIMATION in data_flags:
        target.animation_pose = source.animation_pose
        target.animation_frame = source.animation_frame

    if DataType.CAMERA_LIGHT in data_flags:
        source_camera: Camera = source.secondary_camera if copy_from in EditMode.ANY_FACE_SECONDARY else source.main_camera
        if copy_to in EditMode.ANY_FACE_MAIN or copy_to in EditMode.ANY_BOX:
            target.main_camera = copy.deepcopy(source_camera)
        elif copy_to in EditMode.ANY_FACE_SECONDARY:
            target.secondary_camera = copy.deepcopy(source_camera)

    if DataType.REMOVED_OBJECTS in data_flags:
        target.removed_objects = source.removed_objects.copy()
    if DataType.TEXTURES in data_flags:
        target.textures = copy.deepcopy(source.textures)

    if DataType.SHADING in data_flags:
        target.shading = source.shading


def is_data_type_allowed_copy(data_type_str: str, copy_from: EditMode, copy_to: EditMode) -> bool:
    return (
        (data_type_str != "REMOVED_OBJECTS" and data_type_str != "TEXTURES") or
        prd.shiny.model is None or
        (copy_from in EditMode.ANY_NORMAL and copy_to in EditMode.ANY_NORMAL) or
        (copy_from in EditMode.ANY_SHINY and copy_to in EditMode.ANY_SHINY)
    )


# From operator_modal_view3d_raycast.py
class PKXCameraFocusOperator(bpy.types.Operator):
    """Move camera focus to a place you leftclick, rightclick or 'esc' to cancel"""
    bl_idname = "view3d.pkx_camera_focus_cast"
    bl_label = "Click Focus"

    @classmethod
    def poll(cls, context):
        return not context.scene.advanced_camera_editing

    def modal(self, context, event):
        if event.type in {'MIDDLEMOUSE', 'WHEELUPMOUSE', 'WHEELDOWNMOUSE'}:
            # allow navigation
            return {'PASS_THROUGH'}
        elif event.type == 'LEFTMOUSE':
            """Run this function on left mouse, execute the ray cast"""
            # get the context arguments
            scene = context.scene
            region = context.region
            rv3d = context.region_data
            coord = event.mouse_region_x, event.mouse_region_y

            # get the ray from the viewport and mouse
            view_vector = bpy_extras.view3d_utils.region_2d_to_vector_3d(region, rv3d, coord)
            ray_origin = bpy_extras.view3d_utils.region_2d_to_origin_3d(region, rv3d, coord)

            ray_target = ray_origin + view_vector

            def visible_objects_and_duplis():
                """Loop over (object, matrix) pairs (mesh only)"""

                depsgraph = context.evaluated_depsgraph_get()
                for dup in depsgraph.object_instances:
                    if dup.is_instance:  # Real dupli instance
                        obj = dup.instance_object
                        yield obj, dup.matrix_world.copy()
                    else:  # Usual object
                        obj = dup.object
                        yield obj, obj.matrix_world.copy()

            def obj_ray_cast(obj, matrix):
                """Wrapper for ray casting that moves the ray into object space"""

                # get the ray relative to the object
                matrix_inv = matrix.inverted()
                ray_origin_obj = matrix_inv @ ray_origin
                ray_target_obj = matrix_inv @ ray_target
                ray_direction_obj = ray_target_obj - ray_origin_obj

                # cast the ray
                success, location, normal, face_index = obj.ray_cast(ray_origin_obj, ray_direction_obj)

                if success:
                    return location, normal, face_index
                else:
                    return None, None, None

            for obj, matrix in visible_objects_and_duplis():
                if obj.type == 'MESH':
                    hit, normal, face_index = obj_ray_cast(obj, matrix)
                    if hit is not None:
                        hit_world = matrix @ hit
                        scene.focus = hit_world

            return {'FINISHED'}
        elif event.type in {'RIGHTMOUSE', 'ESC'}:
            return {'CANCELLED'}

        return {'RUNNING_MODAL'}

    def invoke(self, context, event):
        if context.space_data.type == 'VIEW_3D':
            context.window_manager.modal_handler_add(self)
            return {'RUNNING_MODAL'}
        else:
            self.report({'WARNING'}, "Active space must be a View3d")
            return {'CANCELLED'}


class PKXSaveOperator(bpy.types.Operator):
    """Save to Json"""
    bl_idname = "wm.pkx_save"
    bl_label = "Save PKX Json"

    def execute(self, context):
        if context.scene.advanced_camera_editing:
            sync_camera_to_props(context)
        sync_props_to_prd(context)

        json: str = prd.to_json()
        print(f"Output: {json}")
        # Will not work if debugging, test saves from the C# PKX-IconGen application
        try:
            name: str = prd.output_name or prd.name
            with open('../Temp/' + name + '.json', 'w') as json_file:
                json_file.write(json)
                json_file.close()
                bpy.ops.wm.save_mainfile()
        except:
            print("Something happened while saving JSON.")
            print("Current PRD: " + prd.to_json())
            return {'CANCELLED'}
        return {'FINISHED'}


class PKXSaveQuitOperator(bpy.types.Operator):
    """Save and exit back to PKX-IconGen"""
    bl_idname = "wm.pkx_save_quit"
    bl_label = "Save and quit"

    def execute(self, context):
        # noinspection PyBroadException
        try:
            bpy.ops.wm.pkx_save()
        except:
            return {'CANCELLED'}
        bpy.ops.wm.quit_blender()
        return {'FINISHED'}


class PKXSelectCamera(bpy.types.Operator):
    """Select Camera"""
    bl_idname = "wm.pkx_select_camera"
    bl_label = "Select Camera"

    @classmethod
    def poll(cls, context):
        return context.scene.advanced_camera_editing

    def execute(self, context):
        bpy.ops.object.select_all(action="DESELECT")
        get_camera().select_set(True)
        return {'FINISHED'}


class PKXSelectFocusPoint(bpy.types.Operator):
    """Select Camera Focus Point"""
    bl_idname = "wm.pkx_select_camera_focus_point"
    bl_label = "Select Camera Focus Point"

    @classmethod
    def poll(cls, context):
        return context.scene.advanced_camera_editing

    def execute(self, context):
        bpy.ops.object.select_all(action="DESELECT")
        get_camera_focus().select_set(True)
        return {'FINISHED'}


# Access Functions
def get_edit_mode_str(scene) -> str:
    if scene.main_mode == "FACE":
        return scene.face_mode
    elif scene.main_mode == "BOX":
        return scene.box_mode
    else:
        raise Exception(f"Unknown main mode: {scene.main_mode}")


def get_edit_mode(scene) -> EditMode:
    return EditMode[get_edit_mode_str(scene)]


def get_camera():
    return bpy.data.objects["PKXIconGen_Camera"]


def get_camera_focus():
    return bpy.data.objects["PKXIconGen_FocusPoint"]


def get_camera_light():
    return bpy.data.objects["PKXIconGen_TopLight"]


def get_armature():
    return common.get_armature(prd, mode)


def can_edit(mode: EditMode, secondary_enabled: bool) -> bool:
    return mode in EditMode.ANY_FACE_MAIN or (mode in EditMode.ANY_FACE_SECONDARY and secondary_enabled) or mode in EditMode.ANY_BOX


# Update Functions
def update_main_mode(self, context):
    if self.main_mode == "FACE":
        if self.copy_mode == "FACE_NORMAL" or self.copy_mode == "":
            self.copy_mode = "BOX_FIRST"
        self.face_mode = "FACE_NORMAL"
    elif self.main_mode == "BOX":
        if self.copy_mode == "BOX_FIRST" or self.copy_mode == "":
            self.copy_mode = "FACE_NORMAL"
        self.box_mode = "BOX_FIRST"


def update_mode(self, context):
    if self.advanced_camera_editing:
        sync_camera_to_props(context)
    sync_props_to_prd(context)

    value = get_edit_mode_str(self)
    global mode
    mode = EditMode[value]

    for texture in list(render_textures.values()):
        common.reset_texture_images(texture)

    common.reset_materials_maps()
    reset_texture_props(self)

    common.switch_model(prd.shiny, mode)
    sync_prd_to_props(context)
    sync_props_to_scene(context)


def update_animation_pose(self, context):
    value = self.animation_pose
    armature = get_armature()

    clean_model_path = common.get_relative_asset_path(prd.get_mode_model(mode))
    # While debugging, if an error occurs here, make sure to clear cache between different JSON files
    action = bpy.data.actions[os.path.basename(clean_model_path) + '_Anim 0 ' + str(value)]

    armature.animation_data.action = action


def update_animation_frame(self, context):
    value = self.animation_frame

    context.scene.frame_set(value)


def update_camera_pos(self, context):
    value = self.pos
    camera = get_camera()

    camera.location = value


def update_focus(self, context):
    value = self.focus
    focus = get_camera_focus()

    focus.location = value


def update_camera_is_ortho(self, context):
    value = self.is_ortho
    camera = get_camera()

    camera.data.type = "ORTHO" if value else "PERSP"


def update_camera_fov(self, context):
    value = self.fov
    camera = get_camera()

    camera.data.angle = radians(value)


def update_camera_ortho_scale(self, context):
    value = self.ortho_scale
    camera = get_camera()

    camera.data.ortho_scale = value


def update_light_type(self, context):
    value = self.light_type
    light = get_camera_light()

    light.data.type = value


def update_light_strength(self, context):
    value = self.light_strength
    light = get_camera_light()

    light.data.energy = value


def update_light_color(self, context):
    value = self.light_color
    light = get_camera_light()

    light.data.color = value


def update_light_distance(self, context):
    value = self.light_distance
    light = get_camera_light()

    light.location[2] = value


def update_color1_r(self, context):
    common.update_shiny_color(self.color1_r, ColorChannel.R, ShinyColors.Color1)
def update_color1_g(self, context):
    common.update_shiny_color(self.color1_g, ColorChannel.G, ShinyColors.Color1)
def update_color1_b(self, context):
    common.update_shiny_color(self.color1_b, ColorChannel.B, ShinyColors.Color1)
def update_color1_a(self, context):
    common.update_shiny_color(self.color1_a, ColorChannel.A, ShinyColors.Color1)

def update_color2_r(self, context):
    common.update_shiny_color(self.color2_r, ColorChannel.R, ShinyColors.Color2)
def update_color2_g(self, context):
    common.update_shiny_color(self.color2_g, ColorChannel.G, ShinyColors.Color2)
def update_color2_b(self, context):
    common.update_shiny_color(self.color2_b, ColorChannel.B, ShinyColors.Color2)
def update_color2_a(self, context):
    common.update_shiny_color(self.color2_a, ColorChannel.A, ShinyColors.Color2)


def update_shading(self, context):
    value = self.shading

    common.update_shading(ObjectShading[value])


def update_advanced_camera_editing(self, context):
    value: bool = self.advanced_camera_editing

    camera = get_camera()
    camera_focus = get_camera_focus()

    camera.hide_select = not value
    camera_focus.hide_select = not value

    common.allow_select_all_armatures(not value)

    for area in bpy.context.screen.areas:
        if area.type == "VIEW_3D":
            for space in area.spaces:
                if not space.use_local_camera:
                    space.show_gizmo = value

    if not value:
        bpy.ops.wm.tool_set_by_id(name="builtin.select_box")

        camera.select_set(False)
        camera_focus.select_set(False)

        sync_camera_to_props(context)
    else:
        bpy.ops.wm.tool_set_by_id(name="builtin.move")


# Textures Updates
def update_current_texture_image(self, context):
    value = self.current_texture_image

    if value is not None:
        self.texture_material = None

        texture: Texture = get_texture_obj(value.name)
        self.custom_texture_path = texture.path or ""
        if len(texture.mats) > 0:
            first_material: Material = next(iter(texture.mats))

            # Materials updates are made in update_texture_material()
            self.texture_material = bpy.data.materials[first_material.name]


def update_custom_texture_path(self, context):
    global custom_texture_path_invalid
    global custom_texture_scale_invalid
    global custom_texture_reused

    texture: Texture = get_texture_obj(self.current_texture_image.name)
    value = self.custom_texture_path
    original_img = self.current_texture_image

    if value is not None and value != "":
        texture_path: str = common.get_absolute_asset_path(value)
        custom_texture_path_invalid = not os.path.isfile(texture_path)
        if not custom_texture_path_invalid:
            other_textures = list(render_textures.values())
            other_textures.remove(texture)
            custom_texture_reused = common.is_custom_texture_used(texture_path, other_textures)
            if not custom_texture_reused:
                custom_texture_reused = False
                common.reset_texture_images(texture)
                custom_texture_scale_invalid = not common.set_custom_image(original_img, texture_path)
                if not custom_texture_scale_invalid:
                    texture.path = value
    else:
        custom_texture_path_invalid = False
        custom_texture_scale_invalid = False
        custom_texture_reused = False

        common.reset_texture_images(texture)
        texture.path = None


def update_texture_material(self, context):
    value = self.texture_material

    if value is not None:
        texture: Texture = get_texture_obj(self.current_texture_image.name)
        mat: Optional[Material] = texture.get_material_by_name(value.name)
        if mat is None:
            mat = Material(value.name, Vector2(0, 0))
            texture.mats.append(mat)

        self.texture_mapping = mat.map.to_mathutils_vector()
    else:
        self.texture_mapping = Vector2(0, 0).to_mathutils_vector()


def update_texture_mapping(self, context):
    value = self.texture_mapping

    original_img = self.current_texture_image
    selected_mat = self.texture_material
    if original_img is not None and selected_mat is not None:
        texture: Texture = get_texture_obj(self.current_texture_image.name)
        mat: Optional[Material] = texture.get_material_by_name(self.texture_material.name)
        custom_img = None
        if texture.path is not None:
            custom_img = bpy.data.images.load(filepath=common.get_absolute_asset_path(texture.path), check_existing=True)

        mat.map = Vector2(value[0], value[1])

        common.set_material_map(custom_img or original_img, self.texture_material, value[0], value[1])


def get_texture_obj(name: str) -> Texture:
    if name not in render_textures.keys():  # Should only be the case when selecting the image
        render_textures[name] = Texture(name, None, get_initial_texture_materials(name))

    return render_textures[name]


def get_initial_texture_materials(name: str) -> list[Material]:
    mats_data: list[Material] = list[Material]()

    image_obj = bpy.data.images[name]

    for (mat_name, node, _) in common.pkx_cache.img_mat_mapping[image_obj.name]:
        x = node.inputs[1].default_value[0]
        y = node.inputs[1].default_value[1]
        mats_data.append(Material(mat_name, Vector2(x, y)))

    return mats_data


def make_texture_dict(textures: List[Texture]) -> dict[str, Texture]:
    texture_dict: dict[str, Texture] = dict[str, Texture]()
    for texture in textures:
        texture_dict[texture.name] = texture
    return texture_dict


def reset_texture_props(scene):
    scene.current_texture_image = None
    scene.texture_material = None


# State sync functions
def sync_camera_to_props(context=None):
    scene = context.scene if context is not None else bpy.context.scene
    camera = get_camera()
    camera_focus = get_camera_focus()

    scene.pos = camera.location
    scene.focus = camera_focus.location
    scene.fov = round(degrees(camera.data.angle))
    scene.ortho_scale = camera.data.ortho_scale


def sync_props_to_scene(context=None):
    scene = context.scene if context is not None else bpy.context.scene
    armature = get_armature()
    camera = get_camera()
    camera_focus = get_camera_focus()
    camera_light = get_camera_light()

    camera_light.data.type = scene.light_type
    camera_light.data.energy = scene.light_strength
    camera_light.data.color = scene.light_color
    camera_light.location[2] = scene.light_distance

    camera.location = scene.pos
    camera_focus.location = scene.focus
    camera.data.type = "ORTHO" if scene.is_ortho else "PERSP"
    camera.data.angle = radians(scene.fov)
    camera.data.ortho_scale = scene.ortho_scale

    clean_model_path = common.get_relative_asset_path(prd.get_mode_model(mode))
    armature.animation_data.action = bpy.data.actions[
        os.path.basename(clean_model_path) + '_Anim 0 ' + str(scene.animation_pose)]
    scene.frame_set(scene.animation_frame)

    common.show_armature(armature, True)
    common.remove_objects(removed_objects)

    if prd.shiny.color1 is not None and prd.shiny.color2 is not None:
        common.update_all_shiny_colors(
            ShinyColor(scene.color1_r, scene.color1_g, scene.color1_b, scene.color1_a),
            ShinyColor(scene.color2_r, scene.color2_g, scene.color2_b, scene.color2_a)
        )

    common.set_textures(list(render_textures.values()))

    common.update_shading(ObjectShading[scene.shading])


def sync_prd_to_props(context=None):
    global removed_objects
    global render_textures
    scene = context.scene if context is not None else bpy.context.scene

    prd_camera: Optional[Camera] = prd.get_mode_camera(mode)
    animation_pose: Optional[int] = prd.get_mode_animation_pose(mode)
    animation_frame: Optional[int] = prd.get_mode_animation_frame(mode)

    if prd_camera is None:
        prd_camera = Camera.default(RenderTarget[scene.main_mode])

    scene.pos = prd_camera.pos.to_mathutils_vector()
    scene.focus = prd_camera.focus.to_mathutils_vector()
    scene.is_ortho = prd_camera.is_ortho
    scene.fov = prd_camera.fov
    scene.ortho_scale = prd_camera.ortho_scale

    prd_light: Light = prd_camera.light
    scene.light_type = prd_light.type.name
    scene.light_strength = prd_light.strength
    scene.light_color = prd_light.color.to_mathutils_vector()
    scene.light_distance = prd_light.distance

    scene.animation_pose = animation_pose
    scene.animation_frame = animation_frame

    removed_objects = prd.get_mode_removed_objects(mode)

    if prd.shiny.color1 is not None and prd.shiny.color2 is not None:
        scene.color1_r = prd.shiny.color1.r
        scene.color1_g = prd.shiny.color1.g
        scene.color1_b = prd.shiny.color1.b
        scene.color1_a = prd.shiny.color1.a

        scene.color2_r = prd.shiny.color2.r
        scene.color2_g = prd.shiny.color2.g
        scene.color2_b = prd.shiny.color2.b
        scene.color2_a = prd.shiny.color2.a

    render_textures = make_texture_dict(prd.get_mode_textures(mode))

    scene.shading = prd.get_mode_shading(mode).name


def sync_props_to_prd(context):
    global prd
    scene = context.scene

    camera_pos = scene.pos
    camera_focus_pos = scene.focus

    camera_pos_vector: Vector3 = Vector3(camera_pos[0], camera_pos[1], camera_pos[2])
    camera_focus_pos_vector: Vector3 = Vector3(camera_focus_pos[0], camera_focus_pos[1], camera_focus_pos[2])
    is_ortho: bool = scene.is_ortho
    fov: float = scene.fov
    ortho_scale: float = scene.ortho_scale

    light_color_list = scene.light_color
    light_type: LightType = LightType[scene.light_type]
    light_strength: float = scene.light_strength
    light_color: Color = Color(light_color_list[0], light_color_list[1], light_color_list[2], 1)
    light_distance: float = scene.light_distance
    light: Light = Light(light_type, light_strength, light_color, light_distance)

    secondary_enabled: bool = scene.secondary_enabled

    animation_pose: float = scene.animation_pose
    animation_frame: float = scene.animation_frame

    removed_objs: list[str] = list(removed_objects)

    textures: list[Texture] = list(render_textures.values())

    shading: ObjectShading = ObjectShading[scene.shading]

    prd_camera: Optional[Camera]
    if mode in EditMode.ANY_FACE_SECONDARY and not secondary_enabled:
        prd_camera = None
    else:
        prd_camera = Camera(camera_pos_vector, camera_focus_pos_vector, is_ortho, fov, ortho_scale, light)

    def update_render(prd_render_data: RenderData):
        if mode in EditMode.ANY_FACE_SECONDARY:
            prd_render_data.secondary_camera = prd_camera
        else:
            prd_render_data.main_camera = prd_camera
        prd_render_data.animation_pose = animation_pose
        prd_render_data.animation_frame = animation_frame
        prd_render_data.removed_objects = removed_objs
        prd_render_data.textures = textures
        prd_render_data.shading = shading

    prd.update_mode_render(mode, update_render)

    if prd.shiny.color1 is not None and prd.shiny.color2 is not None:
        prd.shiny.color1 = ShinyColor(scene.color1_r, scene.color1_g, scene.color1_b, scene.color1_a)
        prd.shiny.color2 = ShinyColor(scene.color2_r, scene.color2_g, scene.color2_b, scene.color2_a)


# Props
data_type_defs = {
    'ANIMATION': ('ANIMATION', "Animation", "Copy Animation Pose and Frame", DataType.ANIMATION.value),
    'CAMERA_LIGHT': ('CAMERA_LIGHT', "Camera/Light", "Copy Camera, Focus Point and Light", DataType.CAMERA_LIGHT.value),
    'SHADING': ('SHADING', "Shading", "Copy Shading", DataType.SHADING.value),
    'REMOVED_OBJECTS': ('REMOVED_OBJECTS', "Removed Objects", "Copy Removed Objects", DataType.REMOVED_OBJECTS.value),
    'TEXTURES': ('TEXTURES', "Textures", "Copy Textures", DataType.TEXTURES.value)
}

edit_mode_defs = {
    'FACE_NORMAL': ('FACE_NORMAL', "Normal", "Regular face icon"),
    'FACE_NORMAL_SECONDARY': ('FACE_NORMAL_SECONDARY', "Normal Secondary", "Regular secondary side for asymmetric Pokemon like Zangoose"),
    'FACE_SHINY': ('FACE_SHINY', "Shiny", "Shiny face icon"),
    'FACE_SHINY_SECONDARY': ('FACE_SHINY_SECONDARY', "Shiny Secondary", "Shiny secondary side for asymmetric Pokemon like Zangoose"),

    'BOX_FIRST': ('BOX_FIRST', "First", "First box icon. Colosseum only uses this frame"),
    'BOX_FIRST_SHINY': ('BOX_FIRST_SHINY', "First Shiny", "First shiny box icon. Colosseum only uses this frame"),
    'BOX_SECOND': ('BOX_SECOND', "Second", "Second box icon"),
    'BOX_SECOND_SHINY': ('BOX_SECOND_SHINY', "Second Shiny", "Second shiny box icon"),
    'BOX_THIRD': ('BOX_THIRD', "Third", "Third box icon"),
    'BOX_THIRD_SHINY': ('BOX_THIRD_SHINY', "Third Shiny", "Third shiny box icon")
}

MAINPROPS = [
    ('main_mode',
     bpy.props.EnumProperty(
         name="Main mode",
         description="Main mode",
         items=[
             ('FACE', "Face", "Change Face icons"),
             ('BOX', "Box", "Change Box Icons")
         ],
         
         update=update_main_mode
     )),
    ('face_mode',
     bpy.props.EnumProperty(
         name="Mode",
         description="Face edit mode",
         items=[
            edit_mode_defs["FACE_NORMAL"],
            edit_mode_defs["FACE_NORMAL_SECONDARY"],
            edit_mode_defs["FACE_SHINY"],
            edit_mode_defs["FACE_SHINY_SECONDARY"]
         ],
         
         update=update_mode
     )),
    ('box_mode',
     bpy.props.EnumProperty(
         name="Mode",
         description="Box edit mode",
         items=[
            edit_mode_defs["BOX_FIRST"],
            edit_mode_defs["BOX_FIRST_SHINY"],
            edit_mode_defs["BOX_SECOND"],
            edit_mode_defs["BOX_SECOND_SHINY"],
            edit_mode_defs["BOX_THIRD"],
            edit_mode_defs["BOX_THIRD_SHINY"]
         ],
         
         update=update_mode
     )),
    ('secondary_enabled',
     bpy.props.BoolProperty(
         name="Enable secondary cameras",
         description="Enable the secondary cameras for asymmetric Pokemon like Zangoose"
     ))
]

ANIMATIONPROPS = [
    ('animation_pose',
     bpy.props.IntProperty(
         name="Animation Pose",
         description="Animation pose like idle, attacking, dying, etc... Last 2-3 poses are usually empty",
         min=0,
         max=len(bpy.data.actions) if len(bpy.data.actions) != 0 else 50,  # TODO Fix
         
         update=update_animation_pose
     )),
    ('animation_frame',
     bpy.props.IntProperty(
         name="Animation Frame",
         description="Animation frame of the pose",
         min=0,
         max=1000,
         soft_min=0,
         soft_max=500,
         
         update=update_animation_frame
     ))
]

CAMERAPROPS = [
    ('pos',
     bpy.props.FloatVectorProperty(
         name='Position',
         description="Position of the camera",
         subtype="XYZ",
         unit="NONE",
         default=(14, -13.5, 5.5),
         
         update=update_camera_pos
     )),
    ('focus',
     bpy.props.FloatVectorProperty(
         name='Focus Position',
         description="Position where the camera should look at",
         subtype="XYZ",
         unit="NONE",
         default=(0, 0, 0),
         
         update=update_focus
     )),
    ('is_ortho',
     bpy.props.BoolProperty(
         name='Use Orthographic Camera',
         description="Use an orthographic camera instead of a perspective camera",
         default=True,
         
         update=update_camera_is_ortho
     )),
    ('fov',
     bpy.props.IntProperty(
         name='Field of View',
         description="Field of view of the Camera in degrees",
         subtype="ANGLE",
         min=0,
         default=40,
         
         update=update_camera_fov
     )),
    ('ortho_scale',
     bpy.props.FloatProperty(
         name='Orthographic Scale (Zoom)',
         description="\"Zoom\" of an orthographic camera",
         min=0,
         default=7.31429,
         
         update=update_camera_ortho_scale
     )),
    ('advanced_camera_editing',
     bpy.props.BoolProperty(
         name="Advanced Camera Editing",
         description="Allows editing the camera within the 3D view. This will disable the ability to remove objects, go back to the normal mode to do so",
         default=False,
         
         update=update_advanced_camera_editing
     ))
]

LIGHTPROPS = [
    ('light_type',
     bpy.props.EnumProperty(
         name="Light type",
         description="Type of light, Area lights should usually do the trick",
         items=[
             ('POINT', "Point", "Point light"),
             ('SUN', "Sun", "Sun. I wouldn't use that one if I were you"),
             ('SPOT', "Spot", "Spot light"),
             ('AREA', "Area", "Area light"),
         ],
         default=3,  # Area
         
         update=update_light_type
     )),
    ('light_strength',
     bpy.props.FloatProperty(
         name="Light Strength",
         description="Brightness of the light",
         unit="POWER",
         step=125,
         default=125,
         min=0,
         
         update=update_light_strength
     )),
    ('light_color',
     bpy.props.FloatVectorProperty(
         name="Color",
         description="Color of the light",
         subtype="COLOR",
         default=(1, 1, 1),
         min=0,
         max=1,
         
         update=update_light_color
     )),
    ('light_distance',
     bpy.props.FloatProperty(
         name="Light Distance",
         description="Distance away from the focus point",
         unit="NONE",
         default=5,
         min=0,
         
         update=update_light_distance
     ))
]


def poll_current_texture_image(scene, img):
    model: str = common.get_relative_asset_path(prd.get_mode_model(mode))
    model_file: str = os.path.basename(model)

    return img.type != "RENDER_RESULT" and img.name.startswith(model_file) and img.filepath == ""


def poll_texture_materials(scene, mat_obj):
    original_img = scene.current_texture_image
    texture: Texture = get_texture_obj(original_img.name)
    custom_img = None
    if texture.path is not None:
        custom_img = bpy.data.images.load(filepath=common.get_absolute_asset_path(texture.path), check_existing=True)

    if original_img is not None and mat_obj.node_tree is not None:
        for node in mat_obj.node_tree.nodes:
            if common.is_node_teximage_with_image(node, original_img) or \
                    (custom_img is not None and common.is_node_teximage_with_image(node, custom_img)):
                return True

    return False


TEXTURESPROPS = [
    ('current_texture_image',
     bpy.props.PointerProperty(
         name="Texture",
         description="Texture search",
         type=bpy.types.Image,
         poll=poll_current_texture_image,
         
         update=update_current_texture_image,
     )),
    ('custom_texture_path',
     bpy.props.StringProperty(
         name="Custom Texture",
         description="Path to texture used to replace the original one, must be on the same integer scale as the original texture: 1x, 2x, 3x, etc. {{AssetsPath}} can be used here",
         default="",
         subtype="FILE_PATH",
         
         update=update_custom_texture_path,
     )),
    ('texture_material',
     bpy.props.PointerProperty(
         name="Material",
         description="Material that uses the texture. Used for mapping",
         type=bpy.types.Material,
         poll=poll_texture_materials,
         
         update=update_texture_material,
     )),
    ('texture_mapping',
     bpy.props.FloatVectorProperty(
         name="Mapping",
         description="Direct mapping of the texture for this material",
         size=2,
         step=1,
         subtype="XYZ",
         default=(0, 0),
         min=-50,
         max=50,
         
         update=update_texture_mapping
     ))
]

SHINYPROPS = [
    ('color1_r',
     bpy.props.IntProperty(
         name="R",
         description="Red Color1",
         min=0,
         max=3,
         default=0,
         update=update_color1_r
     )),
    ('color1_g',
     bpy.props.IntProperty(
         name="G",
         description="Green Color1",
         min=0,
         max=3,
         default=1,
         update=update_color1_g
     )),
    ('color1_b',
     bpy.props.IntProperty(
         name="B",
         description="Blue Color1",
         min=0,
         max=3,
         default=2,
         update=update_color1_b
     )),
    ('color1_a',
     bpy.props.IntProperty(
         name="A",
         description="Alpha Color1, changing this is not recommanded",
         min=0,
         max=3,
         default=3,
         update=update_color1_a
     )),
    ('color2_r',
     bpy.props.IntProperty(
         name="R",
         description="Red Color2",
         min=0,
         max=255,
         default=127,
         update=update_color2_r
     )),
    ('color2_g',
     bpy.props.IntProperty(
         name="G",
         description="Green Color2",
         min=0,
         max=255,
         default=127,
         update=update_color2_g
     )),
    ('color2_b',
     bpy.props.IntProperty(
         name="B",
         description="Blue Color2",
         min=0,
         max=255,
         default=127,
         update=update_color2_b
     )),
    ('color2_a',
     bpy.props.IntProperty(
         name="A",
         description="Alpha Color2, changing this is not recommended",
         min=0,
         max=255,
         default=127,
         update=update_color2_a
     ))
]


def set_items_to_copy_defaults() -> None:
    defaults: set[str] = {'ANIMATION', 'CAMERA_LIGHT', 'SHADING'}
    if prd.shiny.model is None:
        defaults.add('REMOVED_OBJECTS')
        defaults.add('TEXTURES')

    ADVANCEDPROPS[1][1].keywords["default"] = defaults


def get_copy_mode_items(self, context) -> list[Optional[tuple]]:
    items: list[Optional[tuple]] = [
        edit_mode_defs['BOX_THIRD_SHINY'],
        edit_mode_defs['BOX_THIRD'],
        edit_mode_defs['BOX_SECOND_SHINY'],
        edit_mode_defs['BOX_SECOND'],
        edit_mode_defs['BOX_FIRST_SHINY'],
        edit_mode_defs['BOX_FIRST'],
        None
    ]
    if self.secondary_enabled:
        items.append(edit_mode_defs['FACE_SHINY_SECONDARY'])
        items.append(edit_mode_defs['FACE_NORMAL_SECONDARY'])
    items.append(edit_mode_defs['FACE_SHINY'])
    items.append(edit_mode_defs['FACE_NORMAL'])

    edit_mode_str = get_edit_mode_str(context.scene)
    if edit_mode_defs[edit_mode_str] in items:
        items.remove(edit_mode_defs[edit_mode_str])

    return items


def set_copy_mode_default(secondary_enabled: bool) -> None:
    ADVANCEDPROPS[2][1].keywords["default"] = 9 if secondary_enabled else 7


ADVANCEDPROPS = [
    ('shading',
     bpy.props.EnumProperty(
         name="Object Shading:",
         description="Determines how the objects are shaded, sometimes Smooth shading can give better results, but can break and look weird in other cases",
         items=[
             ('FLAT', "Flat", "Flat Shading, default and the most stable"),
             ('SMOOTH', "Smooth", "Smooth shading, can give better results for more round Pokemon, can cause visual \"weirdness\".")
         ],
         default=0,  # Flat
         
         update=update_shading
     )),

    ('items_to_copy',
     bpy.props.EnumProperty(
         name="Copy items",
         description="Item to copy to target, hold shift to multi-select",
         items=[
            data_type_defs['ANIMATION'],
            data_type_defs['CAMERA_LIGHT'],
            data_type_defs['SHADING'],
            data_type_defs['REMOVED_OBJECTS'],
            data_type_defs['TEXTURES']
         ],
         options={'ENUM_FLAG'},
         default={'ANIMATION', 'CAMERA_LIGHT', 'SHADING'}
     )),
    ('copy_mode',
     bpy.props.EnumProperty(
         name="Copy to/from",
         description="Copy data to or from, see button tooltips for info",
         items=get_copy_mode_items,
         options=set(),
         default=99  # 'FACE_SHINY'
     ))
]

ALLPROPS = [
    MAINPROPS,
    ANIMATIONPROPS,
    CAMERAPROPS,
    LIGHTPROPS,
    TEXTURESPROPS,
    SHINYPROPS,
    ADVANCEDPROPS
]


# Panels
class PKXPanel:
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'PKX-IconGen'


class PKXMainPanel(PKXPanel, bpy.types.Panel):
    bl_idname = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_label = f"PKX-IconGen {addon_ver_str}"
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        layout = self.layout
        col = layout.column()
        main_mode = context.scene.main_mode

        layout.use_property_split = True
        layout.use_property_decorate = False
        for (prop_name, _) in MAINPROPS:
            expand = prop_name == "main_mode" or prop_name == "face_mode" or prop_name == "box_mode"

            if prop_name == "main_mode":
                col.label(text=f"Main mode")
                col.row(align=True).prop(context.scene, prop_name, expand=expand)
            elif main_mode == "FACE" and (prop_name == "face_mode" or prop_name == "secondary_enabled"):
                if prop_name == "face_mode":
                    col.label(text=f"Face mode")
                col.row(align=True).prop(context.scene, prop_name, expand=expand)
            elif main_mode == "BOX" and prop_name == "box_mode":
                col.label(text=f"Box mode")
                col.row(align=True).prop(context.scene, prop_name, expand=expand)

        col.separator()
        col.operator(PKXDeleteOperator.bl_idname)
        col.operator(PKXResetDeletedOperator.bl_idname)
        col.operator(PKXSaveQuitOperator.bl_idname)


class PKXAnimationPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_ANIMATION_PANEL'
    bl_label = 'Animation/Pose'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        init_simple_subpanel(self, context, ANIMATIONPROPS)


class PKXCameraPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_CAMERA_PANEL'
    bl_label = 'Camera'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        col = layout.column()
        col.enabled = can_edit(mode, scene.secondary_enabled) and not scene.advanced_camera_editing
        for (prop_name, _) in CAMERAPROPS:
            if prop_name == "focus":
                row = col.row(align=True)
                row.prop(scene, prop_name)
                col.operator(PKXCameraFocusOperator.bl_idname)
            elif prop_name == "fov":
                if not scene.is_ortho:
                    row = col.row(align=True)
                    row.prop(scene, prop_name)
            elif prop_name == "ortho_scale":
                if scene.is_ortho:
                    row = col.row(align=True)
                    row.prop(scene, prop_name)
            elif prop_name != "advanced_camera_editing":
                row = col.row(align=True)
                row.prop(scene, prop_name)
        col.separator()
        col = layout.column()
        label = "Stop using advanced camera editing" if scene.advanced_camera_editing else "Use advanced camera editing"
        col.prop(scene, 'advanced_camera_editing', text=label, toggle=True)
        if scene.advanced_camera_editing:
            row = col.row(align=True)
            row.operator(PKXSelectCamera.bl_idname)
            row.operator(PKXSelectFocusPoint.bl_idname)


class PKXLightPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_LIGHT_PANEL'
    bl_label = 'Light'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        init_simple_subpanel(self, context, LIGHTPROPS)


class PKXTexturesPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_TEXTURES_PANEL'
    bl_label = 'Textures'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        col = layout.column()
        col.enabled = can_edit(mode, scene.secondary_enabled)
        for (prop_name, _) in TEXTURESPROPS:
            if prop_name == "current_texture_image":
                row = col.row(align=True)
                row.prop(scene, prop_name)

                if scene.current_texture_image is not None:
                    col.template_icon(icon_value=scene.current_texture_image.preview.icon_id, scale=5)
                    row = col.row(align=True)
                    row.label(text=f"Regular texture size: {scene.current_texture_image.size[0]}x{scene.current_texture_image.size[1]}")
            else:
                image_col = col.column()
                image_col.enabled = scene.current_texture_image is not None

                if prop_name == "custom_texture_path":
                    row = image_col.row(align=True)
                    row.prop(scene, prop_name)

                    if custom_texture_path_invalid:
                        row = image_col.row(align=True)
                        row.label(text="Path is invalid, texture will not be saved until the path is empty or valid.", icon="ERROR")
                    if custom_texture_scale_invalid:
                        row = image_col.row(align=True)
                        row.label(text="Scale is invalid, texture will not be saved until the scale is an integer scale: 1x, 2x, 3x, etc.", icon="ERROR")
                    if custom_texture_reused:
                        row = image_col.row(align=True)
                        row.label(text="Custom texture cannot be reused due to limitations. Make a copy of the texture and use the copy.", icon="ERROR")
                    row = image_col.row(align=True)
                    row.operator(PKXReplaceByAssetsPathOperator.bl_idname)
                    image_col.separator()
                elif prop_name == "texture_material":
                    row = image_col.row(align=True)
                    row.prop(scene, prop_name)
                elif prop_name == "texture_mapping":
                    row = image_col.row(align=True)
                    row.enabled = scene.texture_material is not None
                    row.prop(scene, prop_name)


class PKXShinyPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_SHINY_PANEL'
    bl_label = 'Shiny'
    bl_options = {'DEFAULT_CLOSED'}

    @classmethod
    def poll(cls, context):
        return prd.shiny.color1 is not None and prd.shiny.color2 is not None and (mode in EditMode.ANY_SHINY)

    def draw(self, context):
        col = self.layout.column()
        col.separator()
        row = col.row(align=True)
        row.label(text="Color1: ")
        row.prop(context.scene, "color1_r")
        row.prop(context.scene, "color1_g")
        row.prop(context.scene, "color1_b")
        row.prop(context.scene, "color1_a")
        col.separator()
        row = col.row(align=True)
        row.label(text="Color2: ")
        row.prop(context.scene, "color2_r")
        row.prop(context.scene, "color2_g")
        row.prop(context.scene, "color2_b")
        row.prop(context.scene, "color2_a")


class PKXAdvancedPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_ADVANCED_PANEL'
    bl_label = 'Advanced'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        layout = self.layout

        col = layout.column()
        col.enabled = can_edit(mode, context.scene.secondary_enabled)

        row = col.row()
        row.label(text="Shading:")
        row.prop(context.scene, "shading", expand=True)

        col.separator()

        col.label(text="Copy Tool")
        copy_from: EditMode = get_edit_mode(context.scene)
        copy_to: EditMode = EditMode[context.scene.copy_mode]
        #  https://blender.stackexchange.com/questions/250876/disable-options-on-enumproperty/250922#250922
        row = col.row(align=True)
        for iden in data_type_defs.keys():
            item_layout = row.row(align=True)
            allowed = is_data_type_allowed_copy(iden, copy_from, copy_to)
            item_layout.enabled = allowed
            item_layout.prop_enum(context.scene, "items_to_copy", iden)

        col.prop(context.scene, "copy_mode")
        row = col.row()
        row.operator(PKXCopyToOperator.bl_idname)
        row.operator(PKXCopyFromOperator.bl_idname)


def init_simple_subpanel(self, context, props):
    layout = self.layout

    col = layout.column()
    col.enabled = can_edit(mode, context.scene.secondary_enabled)
    for (prop_name, _) in props:
        row = col.row(align=True)
        row.prop(context.scene, prop_name)

    return col


CLASSES = [
    PKXMainPanel,
    PKXAnimationPanel,
    PKXCameraPanel,
    PKXLightPanel,
    PKXTexturesPanel,
    PKXShinyPanel,
    PKXAdvancedPanel,

    ShowRegionUiOperator,
    PKXSaveOperator,
    PKXSaveQuitOperator,
    PKXDeleteOperator,
    PKXResetDeletedOperator,
    PKXReplaceByAssetsPathOperator,
    PKXCopyToOperator,
    PKXCopyFromOperator,
    PKXCameraFocusOperator,
    PKXSelectCamera,
    PKXSelectFocusPoint
]


# We cannot register without data, addon is not meant to be used standalone anyway
def register(data: PokemonRenderData):
    global prd
    prd = data
    secondary_enabled = prd.face.secondary_camera is not None

    set_items_to_copy_defaults()
    set_copy_mode_default(secondary_enabled)
    for prop_list in ALLPROPS:
        for (prop_name, prop_value) in prop_list:
            setattr(bpy.types.Scene, prop_name, prop_value)

    for c in CLASSES:
        bpy.utils.register_class(c)

    scene = bpy.data.scenes["Scene"]
    scene.secondary_enabled = secondary_enabled

    sync_prd_to_props()
    sync_props_to_scene()

    # Unselect on start
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    # Key binds
    wm = bpy.context.window_manager

    km = wm.keyconfigs.addon.keymaps.new(name='Object Mode', space_type='EMPTY')
    kmi = km.keymap_items.new(PKXDeleteOperator.bl_idname, 'DEL', 'PRESS')
    addon_keymaps.append((km, kmi))

    km = wm.keyconfigs.addon.keymaps.new(name='Window', space_type='EMPTY')
    kmi = km.keymap_items.new(PKXSaveOperator.bl_idname, 'S', 'PRESS', ctrl=1)
    addon_keymaps.append((km, kmi))

    # Deactivate object context menu
    for b in wm.keyconfigs.active.keymaps["Object Mode"].keymap_items:
        if b is not None and b.name == "Object Context Menu":
            b.active = False


def unregister():
    for prop_list in ALLPROPS:
        for (prop_name, _) in prop_list:
            delattr(bpy.types.Scene, prop_name)

    for c in CLASSES:
        bpy.utils.unregister_class(c)

    # key binds
    for km, kmi in addon_keymaps:
        km.keymap_items.remove(kmi)
    addon_keymaps.clear()

    bpy.ops.wm.show_region_ui()


if __name__ == "__main__":
    print("Cannot be used standalone, use with the PKX-IconGen app.")
