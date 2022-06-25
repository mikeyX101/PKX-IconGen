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
import sys
import os

sys.path.append(os.getcwd())

from math import radians
from data.camera import Camera
from data.light import Light
from data.pokemon_render_data import PokemonRenderData
from data.edit_mode import EditMode
from data.render_job import RenderJob
import utils


def get_armature(prd: PokemonRenderData, mode: EditMode):
    objs = bpy.data.objects
    if prd.shiny.render.model != "" and (mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY):
        armature = objs["Armature1"]
    else:
        armature = objs["Armature0"]

    return armature


def sync_prd_to_scene(prd: PokemonRenderData, mode: EditMode):
    utils.switch_model(prd.shiny, mode)

    objs = bpy.data.objects
    scene = bpy.data.scenes["Scene"]
    armature = get_armature(prd, mode)
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

    armature.animation_data.action = bpy.data.actions[os.path.basename(prd.get_mode_render(mode).model) + '_Anim 0 ' + str(animation_pose)]
    scene.frame_set(animation_frame)

    utils.remove_objects(prd.get_mode_removed_objects(mode))


if __name__ == "__main__":
    job: RenderJob = RenderJob.from_json(sys.stdin.readline())
    utils.import_model(job.data.render.model, job.data.shiny.hue)

    blender_render = bpy.data.scenes["Scene"].render

    base_resolution = 48 if job.game == 3 else 42  # For PBR, for Colo/XD
    blender_render.resolution_x = base_resolution * job.scale
    blender_render.resolution_y = base_resolution * job.scale

    sync_prd_to_scene(job.data, EditMode.NORMAL)
    blender_render.filepath = job.main_path
    bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

    if job.data.render.secondary_camera is not None:
        sync_prd_to_scene(job.data, EditMode.NORMAL_SECONDARY)
        blender_render.filepath = job.secondary_path
        bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

    sync_prd_to_scene(job.data, EditMode.SHINY)
    blender_render.filepath = job.shiny_path
    bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

    if job.data.shiny.render.secondary_camera is not None:
        sync_prd_to_scene(job.data, EditMode.SHINY_SECONDARY)
        blender_render.filepath = job.shiny_secondary_path
        bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)
