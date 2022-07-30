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
import bpy.utils.previews
import os

import blender_compat
import utils
from data.color import Color
from data.light import Light, LightType
from data.pokemon_render_data import PokemonRenderData
from data.camera import Camera
from data.texture import Texture
from data.vector2 import Vector2
from data.vector3 import Vector3
from data.edit_mode import EditMode
from math import radians
from math import degrees

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "version": (0, 7, 0),
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
preview_textures = bpy.utils.previews.new()
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
        return len(context.scene.custom_texture_path) == 0 or (not custom_texture_path_invalid and not custom_texture_scale_invalid)

    def execute(self, context):
        if len(context.scene.custom_texture_path) == 0:
            context.scene.custom_texture_path = utils.assets_path + "/icon-gen/"
        else:
            context.scene.custom_texture_path = context.scene.custom_texture_path.replace(utils.assets_path, "{{AssetsPath}}")
        return {'FINISHED'}


class PKXSyncOperator(bpy.types.Operator):
    """Synchronize scene data with the addon, useful if you're doing edits outside the addon.\nUSE AT YOUR OWN RISK, NOT EVERYTHING IS SUPPORTED."""
    bl_idname = "wm.pkx_sync"
    bl_label = "Synchronize"

    def execute(self, context):
        sync_scene_to_props()
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
        utils.show_armature(get_armature())

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
        utils.show_armature(get_armature())
        utils.remove_objects(removed_objects)

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
            utils.reset_texture_images(texture)

        texture_copies: List[Texture] = list()
        for texture in prd.render.textures:
            texture_copies.append(copy.deepcopy(texture))
        render_textures = make_texture_dict(texture_copies)
        utils.set_textures(texture_copies)
        context.scene.current_texture_image = None

        return {'FINISHED'}


class PKXSaveOperator(bpy.types.Operator):
    """Save and exit back to PKX-IconGen"""
    bl_idname = "wm.pkx_save"
    bl_label = "Save and quit"

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
            bpy.ops.wm.quit_blender()
        except:
            print("Something happened while saving JSON.")
            print("Current PRD: " + prd.to_json())
            return {'CANCELLED'}
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

    armature = utils.get_armature(prd, mode)

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
        utils.reset_texture_images(texture)
    self.current_texture_image = None

    utils.switch_model(prd.shiny, mode)
    sync_prd_to_props()
    sync_props_to_scene()


def update_animation_pose(self, context):
    value = self.animation_pose
    armature = get_armature()

    clean_model_path = utils.get_relative_asset_path(prd.get_mode_model(mode))
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


def update_shiny_color(self, context):
    value = self.shiny_color

    utils.change_shiny_color(value)


# Textures Updates
def update_current_texture_image(self, context):
    value = self.current_texture_image

    if value is not None:
        if value.name not in preview_textures:
            preview_tex = preview_textures.new(value.name)
            preview_tex.image_size = value.size
            preview_tex.image_pixels_float = value.pixels

        texture: Texture = get_texture_obj(value.name)
        self.custom_texture_path = texture.path or ""
        if len(texture.maps) > 0:
            first_material_name: str = next(iter(texture.maps.keys()))
            first_material_mapping: Vector2 = texture.maps[first_material_name]

            self.texture_material = bpy.data.materials[first_material_name]
            self.texture_mapping = first_material_mapping.to_mathutils_vector()


def update_custom_texture_path(self, context):
    global custom_texture_path_invalid
    global custom_texture_scale_invalid

    texture: Texture = get_texture_obj(self.current_texture_image.name)
    value = self.custom_texture_path
    original_img = self.current_texture_image

    if value is not None and value != "":
        texture_path: str = utils.get_absolute_asset_path(value)
        if os.path.isfile(texture_path):
            custom_texture_path_invalid = False
            if utils.set_custom_image(original_img, texture_path):
                texture.path = value
                custom_texture_scale_invalid = False
            else:
                custom_texture_scale_invalid = True
        else:
            custom_texture_path_invalid = True
    else:
        custom_texture_path_invalid = False

        utils.reset_texture_images(texture)
        texture.path = None


def update_texture_material(self, context):
    value = self.texture_material

    if value is not None:
        texture: Texture = get_texture_obj(self.current_texture_image.name)
        if value.name not in texture.maps.keys():
            new_mapping: Vector2 = Vector2(0, 0)
            texture.maps[value.name] = new_mapping

        self.texture_mapping = texture.maps[value.name].to_mathutils_vector()


def update_texture_mapping(self, context):
    value = self.texture_mapping

    original_img = self.current_texture_image
    texture: Texture = get_texture_obj(self.current_texture_image.name)
    custom_img = None
    if texture.path is not None:
        custom_img = bpy.data.images.load(filepath=utils.get_absolute_asset_path(texture.path), check_existing=True)

    texture.maps[self.texture_material.name] = Vector2(value[0], value[1])

    utils.set_material_map(custom_img or original_img, self.texture_material, value[0], value[1])


def get_texture_obj(name: str) -> Texture:
    if name not in render_textures.keys():  # Should only be the case when selecting the image
        render_textures[name] = Texture(name, None, get_initial_texture_mapping(name))

    return render_textures[name]


def get_initial_texture_mapping(name: str) -> dict[str, Vector2]:
    mat_maps: dict[str, Vector2] = dict[str, Vector2]()

    image_obj = bpy.data.images[name]
    mats = utils.get_image_materials(image_obj)

    for mat in mats:
        if mat.node_tree is not None:
            for node in mat.node_tree.nodes:
                if utils.is_node_teximage_with_image(node, image_obj):
                    img_vector_input_node = node.inputs[blender_compat.tex_image_in.vector].links[0].from_node
                    if img_vector_input_node.bl_idname == "ShaderNodeMapping":
                        x = img_vector_input_node.inputs[1].default_value[0]
                        y = img_vector_input_node.inputs[1].default_value[1]
                        mat_maps[mat.name] = Vector2(x, y)

    return mat_maps


def make_texture_dict(textures: List[Texture]) -> dict[str, Texture]:
    texture_dict: dict[str, Texture] = dict[str, Texture]()
    for texture in textures:
        texture_dict[texture.name] = texture
    return texture_dict


# State sync functions
# Out of date, see if scene sync is needed/worth to maintain
def sync_scene_to_props():
    scene = bpy.data.scenes["Scene"]
    armature = get_armature()
    camera = get_camera()
    camera_focus = get_camera_focus()
    camera_light = get_camera_light()

    scene.pos = camera.location
    scene.focus = camera_focus.location
    scene.is_ortho = camera.data.type == "ORTHO"
    scene.fov = degrees(camera.data.angle)
    scene.ortho_scale = camera.data.ortho_scale

    scene.light_type = camera_light.data.type
    scene.light_strength = camera_light.data.energy
    scene.light_color = camera_light.data.color
    scene.light_distance = camera_light.location[2]

    action = armature.animation_data.action
    scene.animation_pose = int(action.name[len(action.name) - 1])
    scene.animation_frame = scene.frame_current

    # TODO Shiny color?


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

    clean_model_path = utils.get_relative_asset_path(prd.get_mode_model(mode))
    armature.animation_data.action = bpy.data.actions[
        os.path.basename(clean_model_path) + '_Anim 0 ' + str(scene.animation_pose)]
    scene.frame_set(scene.animation_frame)

    utils.show_armature(armature)
    utils.remove_objects(removed_objects)

    if prd.shiny.hue is not None:
        utils.change_shiny_color(scene.shiny_color)

    utils.set_textures(list(render_textures.values()))


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

    if prd.shiny.hue is not None:
        scene.shiny_color = utils.hue2rgb(prd.shiny.hue)

    render_textures = make_texture_dict(prd.get_mode_textures(mode))


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

    prd_camera: Camera = Camera(camera_pos_vector, camera_focus_pos_vector, is_ortho, fov, ortho_scale, light)

    if mode == EditMode.NORMAL:
        prd.render.main_camera = prd_camera
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = removed_objs
        prd.render.textures = textures

    elif mode == EditMode.NORMAL_SECONDARY:
        if secondary_enabled:
            prd.render.secondary_camera = prd_camera
        else:
            prd.render.secondary_camera = None
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = removed_objs
        prd.render.textures = textures

    elif mode == EditMode.SHINY:
        prd.shiny.render.main_camera = prd_camera
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = removed_objs
        prd.shiny.render.textures = textures

    elif mode == EditMode.SHINY_SECONDARY:
        if secondary_enabled:
            prd.shiny.render.secondary_camera = prd_camera
        else:
            prd.shiny.render.secondary_camera = None
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = removed_objs
        prd.shiny.render.textures = textures

    if prd.shiny.hue is not None:
        prd.shiny.hue = utils.rgb2hue(scene.shiny_color)


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
    model: str = utils.get_relative_asset_path(prd.get_mode_model(mode))
    model_file: str = os.path.basename(model)

    return img.type != "RENDER_RESULT" and img.name.startswith(model_file) and img.filepath == ""


def poll_texture_materials(scene, mat_obj):
    original_img = scene.current_texture_image
    texture: Texture = get_texture_obj(original_img.name)
    custom_img = None
    if texture.path is not None:
        custom_img = bpy.data.images.load(filepath=utils.get_absolute_asset_path(texture.path), check_existing=True)

    if original_img is not None and mat_obj.node_tree is not None:
        for node in mat_obj.node_tree.nodes:
            if utils.is_node_teximage_with_image(node, original_img) or \
                    (custom_img is not None and utils.is_node_teximage_with_image(node, custom_img)):
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
         subtype="XYZ",
         default=(0, 0),
         min=-50,
         max=50,
         update=update_texture_mapping
     )),
]

SHINYPROPS = [
    ('shiny_color',
     bpy.props.FloatVectorProperty(
         name="Color",
         description="Filter to put on top of textures for the shiny effect",
         subtype="COLOR",
         default=(1, 1, 1),
         min=0,
         max=1,
         update=update_shiny_color
     )),
]

ADVANCEDPROPS = [

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
        col.operator(PKXSaveOperator.bl_idname)


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
            if prop_name == "fov":
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
                    preview_tex = preview_textures[scene.current_texture_image.name]
                    col.template_icon(icon_value=preview_tex.icon_id, scale=5)
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
    bl_label = 'Shiny Info'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    @classmethod
    def poll(cls, context):
        return prd.shiny.hue is not None and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY)

    def draw(self, context):
        col = init_simple_subpanel(self, context, SHINYPROPS)
        col.operator(PKXCopyRemovedObjectsOperator.bl_idname)
        col.operator(PKXCopyTexturesOperator.bl_idname)


class PKXAdvancedPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_ADVANCED_PANEL'
    bl_label = 'Advanced'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        col = init_simple_subpanel(self, context, ADVANCEDPROPS)
        col.operator(PKXSyncOperator.bl_idname)


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
    PKXSyncOperator,
    PKXDeleteOperator,
    PKXResetDeletedOperator,
    PKXReplaceByAssetsPathOperator,
    PKXCopyRemovedObjectsOperator,
    PKXCopyTexturesOperator
]


# We cannot register without data, addon is not meant to be used standalone anyway
def register(data: PokemonRenderData):
    global prd
    global preview_textures
    prd = data

    for prop_list in ALLPROPS:
        for (prop_name, prop_value) in prop_list:
            setattr(bpy.types.Scene, prop_name, prop_value)

    for c in CLASSES:
        bpy.utils.register_class(c)

    scene = bpy.data.scenes["Scene"]
    scene.secondary_enabled = data.render.secondary_camera is not None

    sync_prd_to_props()
    utils.remove_objects(removed_objects)

    textures: List[Texture] = list(render_textures.values())
    utils.set_textures(textures)

    # unselect on start
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    # key bind
    wm = bpy.context.window_manager
    km = wm.keyconfigs.addon.keymaps.new(name='Object Mode', space_type='EMPTY')
    kmi = km.keymap_items.new(PKXDeleteOperator.bl_idname, 'DEL', 'PRESS')
    addon_keymaps.append((km, kmi))


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
