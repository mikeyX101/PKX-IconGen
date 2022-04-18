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

from typing import Optional, Set
import bpy
from data.pokemon_render_data import PokemonRenderData
from data.camera import Camera
from data.vector import Vector
import mathutils
from enum import IntEnum
from math import radians
from math import degrees

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "version": (0, 1, 0),
    "category": "User Interface",
    "description": "Addon to help users use PKX-IconGen without any Blender knowledge",
    "author": "Samuel Caron/mikeyX#4697",
    "location": "View3D > PKX-IconGen",
    "doc_url": "https://github.com/mikeyX101/PKX-IconGen"
}

class EditMode(IntEnum):
    NORMAL = 0
    NORMAL_SECONDARY = 1
    SHINY = 2
    SHINY_SECONDARY = 3

addon_keymaps = []

mode: EditMode = EditMode.NORMAL
prd: PokemonRenderData
removed_objects: Set[str] = set()
camera = None

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
    """Synchronize scene data with the addon, useful if you're doing edits outside of the addon"""
    bl_idname = "wm.pkx_sync"
    bl_label = "Synchronize"

    @classmethod
    def poll(cls, context):
        return True

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

        removed_objects.update([obj.name for obj in objs])
        bpy.ops.object.delete()

        print(removed_objects)

        return {'FINISHED'}

class SaveToPKXIconGenOperator(bpy.types.Operator):
    """Save and exit back to PKX-IconGen"""
    bl_idname = "wm.pkx_save"
    bl_label = "Save and quit"

    @classmethod
    def poll(cls, context):
        return True

    def execute(self, context):
        sync_props_to_prd()
        print(prd.to_json())
        try:
            name: str = prd.output_name or prd.name
            with open('../Temp/' + name + '.json', 'w') as json_file:
                json_file.write(prd.to_json())
                json_file.close()
            bpy.ops.wm.quit_blender()
        except:
            print("Something happened while saving JSON.")
            return {'CANCELLED'}
        return {'FINISHED'}

# Access Functions
def get_camera():
    global camera

    if camera is None:
        camera = bpy.data.objects["PKXIconGen_Camera"]
    return camera

def can_edit(mode: EditMode, secondary_enabled: bool) -> bool: 
    return mode == EditMode.NORMAL or mode == EditMode.SHINY or secondary_enabled

# Update Functions
def update_mode(self, context):
    sync_props_to_prd()

    value = self.mode
    global mode
    mode = EditMode[value]

    sync_prd_to_props()
    sync_props_to_scene()

def update_camera_pos(self, context):
    value = self.pos
    camera = get_camera()

    camera.location = value

def update_camera_rot(self, context):
    value = self.rot
    camera = get_camera()

    camera.rotation_euler = value

def update_camera_fov(self, context):
    value = self.fov
    camera = get_camera()

    camera.data.angle = radians(value)

def sync_scene_to_props():
    scene = bpy.data.scenes["Scene"]
    camera = get_camera()

    camera_pos = camera.location
    camera_rot = camera.rotation_euler
    camera_fov = camera.data.angle

    scene["pos"] = camera_pos
    scene["rot"] = camera_rot
    scene["fov"] = degrees(camera_fov)

def sync_props_to_scene():
    scene = bpy.data.scenes["Scene"]
    camera = get_camera()

    camera_pos = scene["pos"]
    camera_rot = scene["rot"]
    camera_fov = scene["fov"]

    camera.location = camera_pos
    camera.rotation_euler = camera_rot
    camera.data.angle = radians(camera_fov)

def sync_prd_to_props():
    scene = bpy.data.scenes["Scene"]

    prd_camera: Optional[Camera] = None
    if mode is EditMode.NORMAL:
        prd_camera = prd.render.main_camera
    elif mode is EditMode.NORMAL_SECONDARY:
        if prd.render.secondary_camera is not None:
            prd_camera = prd.render.secondary_camera
        else:
            prd_camera = Camera.default()
    elif mode is EditMode.SHINY:
        prd_camera = prd.shiny.render.main_camera
    elif mode is EditMode.SHINY_SECONDARY:
        if prd.shiny.render.secondary_camera is not None:
            prd_camera = prd.shiny.render.secondary_camera
        else:
            prd_camera = Camera.default()
    
    scene["pos"] = prd_camera.pos.to_mathutils_vector()
    scene["rot"] = prd_camera.rot.to_mathutils_euler()
    scene["fov"] = prd_camera.fov

def sync_props_to_prd():
    global prd
    scene = bpy.data.scenes["Scene"]
    
    camera_pos = scene["pos"]
    camera_rot = scene["rot"]

    camera_pos_vector = Vector(camera_pos[0], camera_pos[1], camera_pos[2])
    camera_rot_vector = Vector(degrees(camera_rot[0]), degrees(camera_rot[1]), degrees(camera_rot[2]))
    fov = scene["fov"]

    secondary_enabled = scene["secondary_enabled"]

    if mode is EditMode.NORMAL:
        prd.render.main_camera = Camera(camera_pos_vector, camera_rot_vector, fov)
    elif mode is EditMode.NORMAL_SECONDARY:
        if secondary_enabled:
            prd.render.secondary_camera = Camera(camera_pos_vector, camera_rot_vector, fov)
        else:
            prd.render.secondary_camera = None
        
    elif mode is EditMode.SHINY:
        prd.shiny.render.main_camera = Camera(camera_pos_vector, camera_rot_vector, fov)
    elif mode is EditMode.SHINY_SECONDARY:
        if secondary_enabled:
            prd.shiny.render.secondary_camera = Camera(camera_pos_vector, camera_rot_vector, fov)
        else:
            prd.shiny.render.secondary_camera = None

    prd.removed_objects = removed_objects
        
# Props
MAINPROPS = [
    ('mode', 
     bpy.props.EnumProperty(
        name="Mode",
        description="Edit mode",
        items=[ ('NORMAL', "Normal", "Regular icon"),
                ('NORMAL_SECONDARY', "Normal Secondary", "Regular secondary side for asymmetric Pokemon like Zangoose"),
                ('SHINY', "Shiny", "Shiny icon"),
                ('SHINY_SECONDARY', "Shiny Secondary", "Shiny secondary side for asymmetric Pokemon like Zangoose"),],
        update=update_mode
        ),
    ),
    ('secondary_enabled', 
     bpy.props.BoolProperty(
        name="Enable secondary cameras",
        description="Enable the secondary cameras for asymmetric Pokemon like Zangoose"
        ),
    )
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

    ('rot', 
     bpy.props.FloatVectorProperty(
        name='Rotation',
        subtype="EULER",
        unit="ROTATION",
        default=(radians(86.8), radians(0), radians(54)),
        update=update_camera_rot
     )),

    ('fov', 
     bpy.props.IntProperty(
         name='Field of View',
         subtype="ANGLE",
         default=40,
         update=update_camera_fov
     )),
]

ADVANCEDPROPS = [
    
]

ALLPROPS = [
    MAINPROPS, CAMERAPROPS, ADVANCEDPROPS
]

# Panels
class PKXPanel:
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'PKX-IconGen'

class PKXMainPanel(PKXPanel, bpy.types.Panel):
    bl_idname = 'VIEW3D_PT_pkx_main_panel'
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
        col.operator("wm.pkx_delete")
        col.operator("wm.pkx_save")

class PKXCameraPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_pkx_main_panel'
    bl_idname = 'VIEW3D_PT_pkx_camera_panel'
    bl_label = 'Camera'
    bl_options = {'HEADER_LAYOUT_EXPAND'}
    
    def draw(self, context):
        layout = self.layout

        col = layout.column()
        col.enabled = can_edit(EditMode[context.scene.mode], context.scene.secondary_enabled)
        for (prop_name, _) in CAMERAPROPS:
            row = col.row(align=True)
            row.prop(context.scene, prop_name)

class PKXAdvancedPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_pkx_main_panel'
    bl_idname = 'VIEW3D_PT_pkx_advanced_panel'
    bl_label = 'Advanced'
    bl_options = {'DEFAULT_CLOSED'}
    
    def draw(self, context):
        layout = self.layout

        col = layout.column()
        col.enabled = can_edit(EditMode[context.scene.mode], context.scene.secondary_enabled)
        for (prop_name, _) in ADVANCEDPROPS:
            row = col.row(align=True)
            row.prop(context.scene, prop_name)
        col.operator("wm.pkx_sync")

CLASSES = [
    PKXMainPanel,
    PKXCameraPanel,
    PKXAdvancedPanel,
    ShowRegionUiOperator,
    SaveToPKXIconGenOperator,
    PKXSyncOperator,
    PKXDeleteOperator
]

# We cannot register without data, addon is not meant to be used standalone anyways
def register(data: PokemonRenderData):
    global prd 
    prd = data

    for prop_list in ALLPROPS:
        for (prop_name, prop_value) in prop_list:
            setattr(bpy.types.Scene, prop_name, prop_value)
    
    for c in CLASSES:
        bpy.utils.register_class(c)

    scene = bpy.data.scenes["Scene"]
    scene["pos"] = data.render.main_camera.pos.to_mathutils_vector()
    scene["rot"] = data.render.main_camera.rot.to_mathutils_euler()
    scene["fov"] = data.render.main_camera.fov

    objs = bpy.data.objects
    removed_objects.update(prd.removed_objects)
    for obj_to_remove in removed_objects:
        objs.remove(objs[obj_to_remove], do_unlink=True)

    scene["secondary_enabled"] = data.render.secondary_camera is not None

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

if __name__ == "__main__":
    print("Cannot be used standalone, use with the PKX-IconGen app.")