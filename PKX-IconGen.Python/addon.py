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

from asyncore import write
import bpy
from data.pokemon_render_data import PokemonRenderData
from data.vector import Vector
import mathutils
from enum import Enum
from math import radians
from math import degrees
import json

bl_info = {
    "name": "PKX-IconGen Data Interaction",
    "blender": (2, 93, 0),
    "category": "Object",
}

class EditMode(Enum):
    NORMAL = 0
    NORMAL_SECONDARY = 1
    SHINY = 2
    SHINY_SECONDARY = 3


mode: EditMode = EditMode.NORMAL
prd: PokemonRenderData

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

# Operators
class SaveToPKXIconGenOperator(bpy.types.Operator):
    """Save and exit to PKX-IconGen"""
    bl_idname = "wm.pkx_save"
    bl_label = "Save"

    @classmethod
    def poll(cls, context):
        return True

    def execute(self, context):
        save_to_prd()
        try:
            with open('../Temp/' + prd.name + '.json', 'w') as json_file:
                json_file.write(prd.to_json())
                json_file.close()
        except:
            return {'CANCELLED'}
        return {'FINISHED'}

camera = None
# Access Functions
def get_camera():
    global camera

    if camera is None:
        camera = bpy.data.objects["PKXIconGen_Camera"]
    return camera

# Update Functions
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

def save_to_prd():
    scene = bpy.data.scenes["Scene"]

    camera_pos = scene["pos"]
    camera_rot = scene["rot"]

    camera_pos_vector = Vector(camera_pos[0], camera_pos[1], camera_pos[2])
    camera_rot_vector = Vector(degrees(camera_rot[0]), degrees(camera_rot[1]), degrees(camera_rot[2]))
    fov = scene["fov"]
    
    if mode is EditMode.NORMAL:
        prd.render.main_camera.pos = camera_pos_vector
        prd.render.main_camera.rot = camera_rot_vector
        prd.render.main_camera.fov = fov
    elif mode is EditMode.NORMAL_SECONDARY:
        prd.render.secondary_camera.pos = camera_pos_vector
        prd.render.secondary_camera.rot = camera_rot_vector
        prd.render.secondary_camera.fov = fov
    elif mode is EditMode.SHINY:
        prd.shiny.render.main_camera.pos = camera_pos_vector
        prd.shiny.render.main_camera.rot = camera_rot_vector
        prd.shiny.render.main_camera.fov = fov
    elif mode is EditMode.SHINY_SECONDARY:
        prd.shiny.render.secondary_camera.pos = camera_pos_vector
        prd.shiny.render.secondary_camera.rot = camera_rot_vector
        prd.shiny.render.secondary_camera.fov = fov

# Props
MAINPROPS = [
    ('mode', 
     bpy.props.EnumProperty(
        name="Mode:",
        description="Edit mode",
        items=[ ('NORMAL', "Normal", "Regular icon"),
                ('NORMAL_SECONDARY', "Normal Secondary", "Regular secondary side for asymetric Pokemon like Zangoose"),
                ('SHINY', "Shiny", "Shiny icon"),
                ('SHINY_SECONDARY', "Shiny Secondary", "Shiny secondary side for asymetric Pokemon like Zangoose"),]
        )
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

ALLPROPS = [
    MAINPROPS, CAMERAPROPS
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
        col.operator("wm.pkx_save")

class PKXCameraPanel(PKXPanel, bpy.types.Panel):
    bl_parent_id = 'VIEW3D_PT_pkx_main_panel'
    bl_idname = 'VIEW3D_PT_pkx_camera_panel'
    bl_label = 'Camera'
    bl_options = {'DEFAULT_CLOSED'}
    
    def draw(self, context):
        layout = self.layout

        col = layout.column()
        for (prop_name, _) in CAMERAPROPS:
            row = col.row(align=True)
            row.prop(context.scene, prop_name)

CLASSES = [
    PKXMainPanel,
    PKXCameraPanel,
    ShowRegionUiOperator,
    SaveToPKXIconGenOperator
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

def unregister():
    for prop_list in ALLPROPS:
        for (prop_name, _) in prop_list:
            delattr(bpy.types.Scene, prop_name)
    
    for c in CLASSES:
        bpy.utils.unregister_class(c)

if __name__ == "__main__":
    print("Cannot be used standalone, use with the PKX-IconGen app.")