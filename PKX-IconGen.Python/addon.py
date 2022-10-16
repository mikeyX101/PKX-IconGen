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
from data.light import Light, LightType
from data.material import Material
from data.object_shading import ObjectShading
from data.pokemon_render_data import PokemonRenderData
from data.camera import Camera
from data.shiny_color import ColorChannel, ShinyColors, ShinyColor
from data.texture import Texture
from data.vector2 import Vector2
from data.vector3 import Vector3
from data.edit_mode import EditMode
from math import radians
from math import degrees

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "version": (0, 2, 16),
    "category": "User Interface",
    "description": "Addon to help users use PKX-IconGen without any Blender knowledge",
    "author": "Samuel Caron/mikeyX#4697",
    "location": "View3D > PKX-IconGen",
    "doc_url": "https://github.com/mikeyX101/PKX-IconGen"
}

addon_keymaps = []

mode: EditMode = EditMode.NORMAL
prd: PokemonRenderData
removed_objects: List[str] = list()
render_textures: dict[str, Texture] = dict()
custom_texture_path_invalid: bool = False
custom_texture_scale_invalid: bool = False
custom_texture_reused: bool = False
camera = None
camera_focus = None
camera_light = None
armature = None


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
    """Replaces the full AssetsPath with {{AssetsPath}}, path and image needs to be valid. If path is empty, adds the full AssetsPath."""
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
        return len(context.selected_objects) > 0

    def execute(self, context):
        objs = context.selected_objects

        removed_objects.extend([obj.name for obj in objs])
        for obj in objs:
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
        common.show_armature(get_armature())

        return {'FINISHED'}


class PKXCopyRemovedObjectsOperator(bpy.types.Operator):
    """Copy removed objects from the normal version"""
    bl_idname = "wm.pkx_copy_removed_objects"
    bl_label = "Copy removed objects"

    @classmethod
    def poll(cls, context):
        return mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY

    def execute(self, context):
        global removed_objects
        removed_objects = list(prd.render.removed_objects)
        common.show_armature(get_armature())
        common.remove_objects(removed_objects)

        return {'FINISHED'}


class PKXCopyTexturesOperator(bpy.types.Operator):
    """Copy textures from the normal version"""
    bl_idname = "wm.pkx_copy_textures"
    bl_label = "Copy textures"

    @classmethod
    def poll(cls, context):
        return mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY

    def execute(self, context):
        global render_textures
        for texture in list(render_textures.values()):
            common.reset_texture_images(texture)

        texture_copies: List[Texture] = list()
        for texture in prd.render.textures:
            texture_copies.append(copy.deepcopy(texture))
        render_textures = make_texture_dict(texture_copies)

        common.set_textures(texture_copies)
        context.scene.current_texture_image = None

        return {'FINISHED'}


# From operator_modal_view3d_raycast.py
class PKXCameraFocusOperator(bpy.types.Operator):
    """Move camera focus to a place you leftclick, rightclick or 'esc' to cancel"""
    bl_idname = "view3d.pkx_camera_focus_cast"
    bl_label = "Click Focus"

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
        sync_props_to_prd()

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
        try:
            bpy.ops.wm.pkx_save()
        except:
            return {'CANCELLED'}
        bpy.ops.wm.quit_blender()
        return {'FINISHED'}


# Access Functions
def get_camera():
    global camera

    if camera is None:
        camera = bpy.data.objects["PKXIconGen_Camera"]
    return camera


def get_camera_focus():
    global camera_focus

    if camera_focus is None:
        camera_focus = bpy.data.objects["PKXIconGen_FocusPoint"]
    return camera_focus


def get_camera_light():
    global camera_light

    if camera_light is None:
        camera_light = bpy.data.objects["PKXIconGen_TopLight"]
    return camera_light


def get_armature():
    global armature

    armature = common.get_armature(prd, mode)

    return armature


def can_edit(mode: EditMode, secondary_enabled: bool) -> bool:
    return mode == EditMode.NORMAL or mode == EditMode.SHINY or secondary_enabled


# Update Functions
def update_mode(self, context):
    sync_props_to_prd()

    value = self.mode
    global mode
    mode = EditMode[value]

    for texture in list(render_textures.values()):
        common.reset_texture_images(texture)

    reset_texture_props(self)

    common.switch_model(prd.shiny, mode)
    sync_prd_to_props()
    sync_props_to_scene()


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

    common.update_shading(ObjectShading[value], context)


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
    mats = common.get_image_materials(image_obj)

    for mat in mats:
        for node in common.pkx_cache.mapping[mat.name]:
            x = node.inputs[1].default_value[0]
            y = node.inputs[1].default_value[1]
            mats_data.append(Material(mat.name, Vector2(x, y)))

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
def sync_props_to_scene():
    scene = bpy.data.scenes["Scene"]
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

    common.show_armature(armature)
    common.remove_objects(removed_objects)

    if prd.shiny.color1 is not None and prd.shiny.color2 is not None:
        common.update_all_shiny_colors(
            ShinyColor(scene.color1_r, scene.color1_g, scene.color1_b, scene.color1_a),
            ShinyColor(scene.color2_r, scene.color2_g, scene.color2_b, scene.color2_a)
        )

    common.set_textures(list(render_textures.values()))

    common.update_shading(ObjectShading[scene.shading], None)


def sync_prd_to_props():
    global removed_objects
    global render_textures
    scene = bpy.data.scenes["Scene"]

    prd_camera: Optional[Camera] = prd.get_mode_camera(mode)
    animation_pose: Optional[int] = prd.get_mode_animation_pose(mode)
    animation_frame: Optional[int] = prd.get_mode_animation_frame(mode)

    if prd_camera is None:
        prd_camera = Camera.default()

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


def sync_props_to_prd():
    global prd
    scene = bpy.data.scenes["Scene"]

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

    prd_camera: Camera = Camera(camera_pos_vector, camera_focus_pos_vector, is_ortho, fov, ortho_scale, light)

    if mode == EditMode.NORMAL:
        prd.render.main_camera = prd_camera
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = removed_objs
        prd.render.textures = textures
        prd.render.shading = shading

    elif mode == EditMode.NORMAL_SECONDARY:
        if secondary_enabled:
            prd.render.secondary_camera = prd_camera
        else:
            prd.render.secondary_camera = None
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = removed_objs
        prd.render.textures = textures
        prd.render.shading = shading

    elif mode == EditMode.SHINY:
        prd.shiny.render.main_camera = prd_camera
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = removed_objs
        prd.shiny.render.textures = textures
        prd.shiny.render.shading = shading

    elif mode == EditMode.SHINY_SECONDARY:
        if secondary_enabled:
            prd.shiny.render.secondary_camera = prd_camera
        else:
            prd.shiny.render.secondary_camera = None
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = removed_objs
        prd.shiny.render.textures = textures
        prd.shiny.render.shading = shading

    if prd.shiny.color1 is not None and prd.shiny.color2 is not None:
        prd.shiny.color1 = ShinyColor(scene.color1_r, scene.color1_g, scene.color1_b, scene.color1_a)
        prd.shiny.color2 = ShinyColor(scene.color2_r, scene.color2_g, scene.color2_b, scene.color2_a)


# Props
MAINPROPS = [
    ('mode',
     bpy.props.EnumProperty(
         name="Mode",
         description="Edit mode",
         items=[
             ('NORMAL', "Normal", "Regular icon"),
             ('NORMAL_SECONDARY', "Normal Secondary", "Regular secondary side for asymmetric Pokemon like Zangoose"),
             ('SHINY', "Shiny", "Shiny icon"),
             ('SHINY_SECONDARY', "Shiny Secondary", "Shiny secondary side for asymmetric Pokemon like Zangoose"),
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
         max=len(bpy.data.actions) if len(bpy.data.actions) != 0 else 50,
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
     ))
]

LIGHTPROPS = [
    ('light_type',
     bpy.props.EnumProperty(
         name="Light type",
         description="Type of light, Point lights should usually do the trick",
         items=[
             ('POINT', "Point", "Point light"),
             ('SUN', "Sun", "Sun"),
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
         description="Alpha Color2, changing this is not recommanded",
         min=0,
         max=255,
         default=127,
         update=update_color2_a
     ))
]

ADVANCEDPROPS = [
    ('shading',
     bpy.props.EnumProperty(
         name="Object Shading:",
         description="Determines how the objects are shaded, sometimes Smooth shading can give better results, but can break or look weird in other cases",
         items=[
             ('FLAT', "Flat", "Flat Shading, default and the most stable"),
             ('SMOOTH', "Smooth", "Smooth shading, can give better results for more round Pokemon, can cause visual \"weirdness\".")
         ],
         default=0,  # Flat
         update=update_shading
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
    bl_label = 'PKX-IconGen'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        layout = self.layout
        col = layout.column()
        layout.use_property_split = True
        layout.use_property_decorate = False

        col.label(text="PKX-IconGen")
        for (prop_name, _) in MAINPROPS:
            expand = prop_name == "mode"

            row = col.row(align=True)
            row.prop(context.scene, prop_name, expand=expand)
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
        col.enabled = can_edit(EditMode[context.scene.mode], context.scene.secondary_enabled)
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
            else:
                row = col.row(align=True)
                row.prop(scene, prop_name)


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
        col.enabled = can_edit(EditMode[scene.mode], scene.secondary_enabled)
        for (prop_name, _) in TEXTURESPROPS:
            if prop_name == "current_texture_image":
                row = col.row(align=True)
                row.prop(scene, prop_name)

                if scene.current_texture_image is not None:
                    col.template_icon(icon_value=scene.current_texture_image.preview.icon_id, scale=5)
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
                elif prop_name == "texture_mapping":
                    row = image_col.row(align=True)
                    row.enabled = scene.texture_material is not None
                    row.prop(scene, prop_name)
                else:
                    row = image_col.row(align=True)
                    row.prop(scene, prop_name)


class PKXShinyPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_SHINY_PANEL'
    bl_label = 'Shiny'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    @classmethod
    def poll(cls, context):
        return prd.shiny.color1 is not None and prd.shiny.color2 is not None and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY)

    def draw(self, context):
        col = self.layout.column()
        col.operator(PKXCopyRemovedObjectsOperator.bl_idname)
        col.operator(PKXCopyTexturesOperator.bl_idname)
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
        col.enabled = can_edit(EditMode[context.scene.mode], context.scene.secondary_enabled)
        row = col.row()
        row.label(text="Shading:")
        row.prop(context.scene, "shading", expand=True)


def init_simple_subpanel(self, context, props):
    layout = self.layout

    col = layout.column()
    col.enabled = can_edit(EditMode[context.scene.mode], context.scene.secondary_enabled)
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
    PKXCopyRemovedObjectsOperator,
    PKXCameraFocusOperator,
    PKXCopyTexturesOperator
]


# We cannot register without data, addon is not meant to be used standalone anyway
def register(data: PokemonRenderData):
    global prd
    prd = data

    for prop_list in ALLPROPS:
        for (prop_name, prop_value) in prop_list:
            setattr(bpy.types.Scene, prop_name, prop_value)

    for c in CLASSES:
        bpy.utils.register_class(c)

    scene = bpy.data.scenes["Scene"]
    scene.secondary_enabled = data.render.secondary_camera is not None

    sync_prd_to_props()
    common.remove_objects(removed_objects)

    textures: List[Texture] = list(render_textures.values())
    common.set_textures(textures)

    common.update_shading(ObjectShading[scene.shading], None)

    # unselect on start
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    # key bind
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

    # key bind
    for km, kmi in addon_keymaps:
        km.keymap_items.remove(kmi)
    addon_keymaps.clear()

    bpy.ops.wm.show_region_ui()


if __name__ == "__main__":
    print("Cannot be used standalone, use with the PKX-IconGen app.")
