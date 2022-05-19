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

import bpy
from math import radians
from data.camera import Camera
from data.light import Light
from data.pokemon_render_data import PokemonRenderData
from data.edit_mode import EditMode
from utils import import_model
from utils import remove_objects


def sync_prd_to_scene(prd: PokemonRenderData, mode: EditMode):
    objs = bpy.data.objects
    scene = bpy.data.scenes["Scene"]
    armature = objs["Armature0"]
    camera = objs["PKXIconGen_Camera"]
    focus = objs["PKXIconGen_FocusPoint"]
    light = objs["PKXIconGen_TopLight"]

    prd_camera: Camera = prd.get_mode_camera(mode) or Camera.default()
    prd_light: Light = prd_camera.light or Light.default()
    animation_pose: int = prd.get_mode_animation_pose(mode) or 0
    animation_frame: int = prd.get_mode_animation_frame(mode) or 0

    if prd_camera is None:
        prd_camera = Camera.default()

    camera_pos = prd_camera.pos.to_mathutils_vector()
    focus_pos = prd_camera.focus.to_mathutils_vector()
    camera_fov = prd_camera.fov

    camera.location = camera_pos
    focus.location = focus_pos
    camera.data.angle = radians(camera_fov)

    light.data.type = prd_light.type.name
    light.data.energy = prd_light.strength
    light.data.color = prd_light.color.to_list()
    light.location[2] = prd_light.distance

    armature.animation_data.action = bpy.data.actions[animation_pose]
    scene.frame_set(animation_frame)

    remove_objects(prd.get_mode_removed_objects(mode))


#  TODO Vertex Lighting
def change_mats():
    blender_ver = bpy.app.version
    mats = bpy.data.materials

    if (2, 93, 0) <= blender_ver < (2, 94, 0):
        for mat in mats:
            tree = mat.node_tree
            if tree is not None:
                bsdf = tree.nodes["Principled BSDF"]
                bsdf.inputs[4].default_value = 0  # Metallic
                bsdf.inputs[5].default_value = 0  # Specular
                bsdf.inputs[7].default_value = 1  # Roughness
    elif (3, 0, 0) <= blender_ver < (3, 1, 0):  # TODO Validate nodes in 3.1
        for mat in mats:
            tree = mat.node_tree
            if tree is not None:
                bsdf = tree.nodes["Principled BSDF"]
                bsdf.inputs[6].default_value = 0  # Metallic
                bsdf.inputs[7].default_value = 0  # Specular
                bsdf.inputs[9].default_value = 1  # Roughness


def generate_scene(data: PokemonRenderData):
    import_model(data.render.model)
    change_mats()
    sync_prd_to_scene(data, EditMode.NORMAL)
