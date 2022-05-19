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
from data.edit_mode import EditMode
from data.render_job import RenderJob
import scene_generator

job: RenderJob = RenderJob.from_json(sys.stdin.readline())
scene_generator.import_model(job.data.render.model)
scene_generator.change_mats()

blender_render = bpy.data.scenes["Scene"].render

base_resolution = 48 if job.game == 3 else 42  # For PBR, for Colo/XD
blender_render.resolution_x = base_resolution * job.scale
blender_render.resolution_y = base_resolution * job.scale

scene_generator.sync_prd_to_scene(job.data, EditMode.NORMAL)
blender_render.filepath = job.main_path
bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

if job.data.render.secondary_camera is not None:
    scene_generator.sync_prd_to_scene(job.data, EditMode.NORMAL_SECONDARY)
    blender_render.filepath = job.secondary_path
    bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

scene_generator.sync_prd_to_scene(job.data, EditMode.SHINY)
blender_render.filepath = job.shiny_path
bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)

if job.data.shiny.render.secondary_camera is not None:
    scene_generator.sync_prd_to_scene(job.data, EditMode.SHINY_SECONDARY)
    blender_render.filepath = job.shiny_secondary_path
    bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)
