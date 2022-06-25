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

from typing import List, Optional
import bpy
import os

import utils
from data.color import Color
from data.light import Light, LightType
from data.pokemon_render_data import PokemonRenderData
from data.camera import Camera
from data.vector import Vector
from data.edit_mode import EditMode
from math import radians
from math import degrees

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "version": (0, 5, 1),
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


class PKXSyncOperator(bpy.types.Operator):
    """Synchronize scene data with the addon, useful if you're doing edits outside the addon.\nUSE AT YOUR OWN RISK."""
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
        global removed_objects
        objs = context.selected_objects

        removed_objects.extend([obj.name for obj in objs])
        for obj in objs:
            obj.hide_render = True
            obj.hide_viewport = True

        return {'FINISHED'}


class PKXSaveOperator(bpy.types.Operator):
    """Save and exit back to PKX-IconGen"""
    bl_idname = "wm.pkx_save"
    bl_label = "Save and quit"

    def execute(self, context):
        sync_props_to_prd()

        # Will not work if debugging, test saves from the C# PKX-IconGen application
        try:
            name: str = prd.output_name or prd.name
            with open('../Temp/' + name + '.json', 'w') as json_file:
                json_file.write(prd.to_json())
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

    objs = bpy.data.objects
    if prd.shiny.render.model != "" and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY):
        armature = objs["Armature1"]
    else:
        armature = objs["Armature0"]

    return armature


def can_edit(mode: EditMode, secondary_enabled: bool) -> bool:
    return mode == EditMode.NORMAL or mode == EditMode.SHINY or secondary_enabled


# Update Functions
def update_mode(self, context):
    sync_props_to_prd()

    value = self.mode
    global mode
    mode = EditMode[value]

    utils.switch_model(prd.shiny, mode)
    sync_prd_to_props()
    sync_props_to_scene()


def update_animation_pose(self, context):
    value = self.animation_pose
    armature = get_armature()
    action = bpy.data.actions[os.path.basename(prd.get_mode_render(mode).model) + '_Anim 0 ' + str(value)]

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


def update_camera_fov(self, context):
    value = self.fov
    camera = get_camera()

    camera.data.angle = radians(value)


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


def sync_scene_to_props():
    scene = bpy.data.scenes["Scene"]
    armature = get_armature()
    camera = get_camera()
    camera_focus = get_camera_focus()
    camera_light = get_camera_light()

    camera_pos = camera.location
    camera_focus_pos = camera_focus.location
    camera_fov = camera.data.angle

    scene.pos = camera_pos
    scene.focus = camera_focus_pos
    scene.fov = degrees(camera_fov)

    scene.light_type = camera_light.data.type
    scene.light_strength = camera_light.data.energy
    scene.light_color = camera_light.data.color
    scene.light_distance = camera_light.location[2]

    action = armature.animation_data.action
    scene.animation_pose = int(action.name[len(action.name) - 1])
    scene.animation_frame = scene.frame_current

    # TODO Shiny color?


def sync_props_to_scene():
    global removed_objects
    scene = bpy.data.scenes["Scene"]
    armature = get_armature()
    camera = get_camera()
    camera_focus = get_camera_focus()
    camera_light = get_camera_light()

    camera_pos = scene.pos
    camera_focus_pos = scene.focus
    camera_fov = scene.fov

    camera_light.data.type = scene.light_type
    camera_light.data.energy = scene.light_strength
    camera_light.data.color = scene.light_color
    camera_light.location[2] = scene.light_distance

    camera.location = camera_pos
    camera_focus.location = camera_focus_pos
    camera.data.angle = radians(camera_fov)

    armature.animation_data.action = bpy.data.actions[os.path.basename(prd.get_mode_render(mode).model) + '_Anim 0 ' + str(scene.animation_pose)]
    scene.frame_set(scene.animation_frame)

    utils.remove_objects(removed_objects)

    if prd.shiny.hue is not None:
        utils.change_shiny_color(scene.shiny_color)


def sync_prd_to_props():
    global removed_objects
    scene = bpy.data.scenes["Scene"]

    prd_camera: Optional[Camera] = prd.get_mode_camera(mode)
    animation_pose: Optional[int] = prd.get_mode_animation_pose(mode)
    animation_frame: Optional[int] = prd.get_mode_animation_frame(mode)

    if prd_camera is None:
        prd_camera = Camera.default()

    scene.pos = prd_camera.pos.to_mathutils_vector()
    scene.focus = prd_camera.focus.to_mathutils_vector()
    scene.fov = prd_camera.fov

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


def sync_props_to_prd():
    global prd
    scene = bpy.data.scenes["Scene"]

    camera_pos = scene.pos
    camera_focus_pos = scene.focus

    camera_pos_vector: Vector = Vector(camera_pos[0], camera_pos[1], camera_pos[2])
    camera_focus_pos_vector: Vector = Vector(camera_focus_pos[0], camera_focus_pos[1], camera_focus_pos[2])
    fov: float = scene.fov

    light_color_list = scene.light_color
    light_type: LightType = LightType[scene.light_type]
    light_strength: float = scene.light_strength
    light_color: Color = Color(light_color_list[0], light_color_list[1], light_color_list[2])
    light_distance: float = scene.light_distance
    light: Light = Light(light_type, light_strength, light_color, light_distance)

    secondary_enabled: bool = scene.secondary_enabled

    animation_pose: float = scene.animation_pose
    animation_frame: float = scene.animation_frame

    prd_camera: Camera = Camera(camera_pos_vector, camera_focus_pos_vector, fov, light)
    if mode == EditMode.NORMAL:
        prd.render.main_camera = prd_camera
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = list(removed_objects)
    elif mode == EditMode.NORMAL_SECONDARY:
        if secondary_enabled:
            prd.render.secondary_camera = prd_camera
        else:
            prd.render.secondary_camera = None
        prd.render.animation_pose = animation_pose
        prd.render.animation_frame = animation_frame
        prd.render.removed_objects = list(removed_objects)

    elif mode == EditMode.SHINY:
        prd.shiny.render.main_camera = prd_camera
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = list(removed_objects)
    elif mode == EditMode.SHINY_SECONDARY:
        if secondary_enabled:
            prd.shiny.render.secondary_camera = prd_camera
        else:
            prd.shiny.render.secondary_camera = None
        prd.shiny.render.animation_pose = animation_pose
        prd.shiny.render.animation_frame = animation_frame
        prd.shiny.render.removed_objects = list(removed_objects)

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
        subtype="XYZ",
        unit="NONE",
        default=(14, -13.5, 5.5),
        update=update_camera_pos
     )),
    ('focus',
     bpy.props.FloatVectorProperty(
        name='Focus Position',
        description="Brightness of the light",
        subtype="XYZ",
        unit="NONE",
        default=(0, 0, 0),
        update=update_focus
     )),
    ('fov',
     bpy.props.IntProperty(
        name='Field of View',
        description="Field of view of the Camera in degrees",
        subtype="ANGLE",
        min=0,
        default=40,
        update=update_camera_fov
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
    MAINPROPS, ANIMATIONPROPS, CAMERAPROPS, LIGHTPROPS, SHINYPROPS, ADVANCEDPROPS
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
        col.operator(PKXSaveOperator.bl_idname)


class PKXAnimationPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_ANIMATION_PANEL'
    bl_label = 'Animation/Pose'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        init_subpanel(self, context, ANIMATIONPROPS)


class PKXCameraPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_CAMERA_PANEL'
    bl_label = 'Camera'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        init_subpanel(self, context, CAMERAPROPS)


class PKXLightPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_LIGHT_PANEL'
    bl_label = 'Light'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    def draw(self, context):
        init_subpanel(self, context, LIGHTPROPS)


class PKXShinyPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_SHINY_PANEL'
    bl_label = 'Shiny Info'
    bl_options = {'HEADER_LAYOUT_EXPAND'}

    @classmethod
    def poll(cls, context):
        return prd.shiny.hue is not None and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY)

    def draw(self, context):
        init_subpanel(self, context, SHINYPROPS)


class PKXAdvancedPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_PKX_MAIN_PANEL'
    bl_idname = 'VIEW3D_PT_PKX_ADVANCED_PANEL'
    bl_label = 'Advanced'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        col = init_subpanel(self, context, ADVANCEDPROPS)
        col.operator("wm.pkx_sync")


def init_subpanel(self, context, props):
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
    PKXShinyPanel,
    PKXAdvancedPanel,

    ShowRegionUiOperator,
    PKXSaveOperator,
    PKXSyncOperator,
    PKXDeleteOperator
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
    utils.remove_objects(removed_objects)

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
