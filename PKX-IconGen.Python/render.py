""" License 
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2023 Samuel Caron/mikeyx

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
from typing import Optional, List

import bpy
import sys
import os

sys.path.append(os.getcwd())

from math import radians
from data.camera import Camera
from data.game import Game
from data.light import Light
from data.pokemon_render_data import PokemonRenderData
from data.edit_mode import EditMode
from data.render_job import RenderJob
from data.render_target import RenderTarget
import common

last_rendered_mode: Optional[EditMode] = None


def reset_all(prd: PokemonRenderData):
    global last_rendered_mode
    if last_rendered_mode is not None:
        for texture in prd.get_mode_textures(last_rendered_mode):
            common.reset_texture_images(texture)
        common.reset_materials_maps()


def sync_prd_to_scene(prd: PokemonRenderData, mode: EditMode):
    common.switch_model(prd.shiny, mode)

    objs = bpy.data.objects
    scene = bpy.data.scenes["Scene"]
    armature = common.get_armature(prd, mode)
    camera = objs["PKXIconGen_Camera"]
    focus = objs["PKXIconGen_FocusPoint"]
    light = objs["PKXIconGen_TopLight"]

    prd_camera: Camera = prd.get_mode_camera(mode) or Camera.default(RenderTarget[scene.main_mode])
    prd_light: Light = prd_camera.light or Light.default(RenderTarget[scene.main_mode])
    animation_pose: int = prd.get_mode_animation_pose(mode) or 0
    animation_frame: int = prd.get_mode_animation_frame(mode) or 0

    camera.location = prd_camera.pos.to_mathutils_vector()
    focus.location = prd_camera.focus.to_mathutils_vector()
    camera.data.type = "ORTHO" if prd_camera.is_ortho else "PERSP"
    camera.data.angle = radians(prd_camera.fov)
    camera.data.ortho_scale = prd_camera.ortho_scale

    light.data.type = prd_light.type.name
    light.data.energy = prd_light.strength
    light.data.color = prd_light.color.to_list()
    light.location[2] = prd_light.distance

    clean_model_path = common.get_relative_asset_path(prd.get_mode_model(mode))
    armature.animation_data.action = bpy.data.actions[os.path.basename(clean_model_path) + '_Anim 0 ' + str(animation_pose)]
    scene.frame_set(animation_frame)

    common.remove_objects(prd.get_mode_removed_objects(mode))

    common.set_textures(prd.get_mode_textures(mode))

    common.update_shading(prd.get_mode_shading(mode))


def get_mode_base_resolution(mode: EditMode, game: Game) -> int:
    base_resolution: int
    if mode in EditMode.ANY_FACE:
        base_resolution = 48 if game == Game.POKEMONBATTLEREVOLUTION else 42
    elif mode in EditMode.ANY_BOX:
        base_resolution = 54 if game == Game.POKEMONBATTLEREVOLUTION else 64
    else:
        raise Exception(f"Invalid EditMode provided: {mode.name}")
    return base_resolution


def render_job_mode(job: RenderJob, path: str, mode: EditMode):
    global last_rendered_mode

    blender_render = bpy.data.scenes["Scene"].render

    base_resolution = get_mode_base_resolution(mode, job.game)

    blender_render.resolution_x = base_resolution * job.scale
    blender_render.resolution_y = base_resolution * job.scale

    reset_all(job.data)
    sync_prd_to_scene(job.data, mode)
    blender_render.filepath = path
    bpy.ops.render.render(animation=False, write_still=True, use_viewport=True)
    last_rendered_mode = mode


if __name__ == "__main__":
    # noinspection DuplicatedCode
    debug_json: Optional[str] = None
    debug_egg: Optional[str] = None
    job: RenderJob

    common.parse_cmd_args(sys.argv[sys.argv.index("--") + 1:])
    for arg, value in common.cmd_args:
        if arg == "--pkx-debug":
            debug_json = value
        elif arg == "--debug-egg":
            debug_egg = value

    if debug_json is not None:
        common.attach_debugger(debug_egg)

        file = open(debug_json, "r")
        json = file.readline()
        file.close()

        common.print_verbose(f"Rendering: {json}")
        job: RenderJob = RenderJob.from_json(json)
    else:
        json = sys.stdin.readline()

        common.print_verbose(f"Rendering: {json}")
        job: RenderJob = RenderJob.from_json(json)

    common.import_models(job.data)
    render_targets: List[tuple[str, EditMode]] = []
    if RenderTarget.FACE in job.target:
        render_targets.append((job.face_main_path, EditMode.FACE_NORMAL))

        if job.data.face.secondary_camera is not None:
            render_targets.append((job.face_secondary_path, EditMode.FACE_NORMAL_SECONDARY))

        render_targets.append((job.face_shiny_path, EditMode.FACE_SHINY))

        if job.data.shiny.face.secondary_camera is not None:
            render_targets.append((job.face_shiny_secondary_path, EditMode.FACE_SHINY_SECONDARY))

    if RenderTarget.BOX in job.target:
        render_targets.append((job.box_first_main_path, EditMode.BOX_FIRST))
        render_targets.append((job.box_first_shiny_path, EditMode.BOX_FIRST_SHINY))

        if job.game is Game.POKEMONXDGALEOFDARKNESS:
            render_targets.append((job.box_second_main_path, EditMode.BOX_SECOND))
            render_targets.append((job.box_second_shiny_path, EditMode.BOX_SECOND_SHINY))
            render_targets.append((job.box_third_main_path, EditMode.BOX_THIRD))
            render_targets.append((job.box_third_shiny_path, EditMode.BOX_THIRD_SHINY))

    for target in render_targets:
        render_job_mode(job, target[0], target[1])
