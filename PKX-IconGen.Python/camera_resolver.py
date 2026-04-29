""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2026 Samuel Caron/mikeyX#4697

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
import math

import bpy

import common
from common import get_armature_obj
from data.camera import Camera
from data.edit_mode import EditMode
from data.light import Light
from data.pokemon_render_data import PokemonRenderData
from data.render_target import RenderTarget
from data.vector3 import Vector3


def init_cameras(prd: PokemonRenderData):
    for (render_data, render_target, mode) in [
        (prd.face, RenderTarget.FACE, EditMode.FACE_NORMAL),
        (prd.shiny.face, RenderTarget.FACE, EditMode.FACE_SHINY),
        (prd.box.first, RenderTarget.BOX, EditMode.BOX_FIRST),
        (prd.box.second, RenderTarget.BOX, EditMode.BOX_SECOND),
        (prd.box.third, RenderTarget.BOX, EditMode.BOX_THIRD),
        (prd.shiny.box.first, RenderTarget.BOX, EditMode.BOX_FIRST_SHINY),
        (prd.shiny.box.second, RenderTarget.BOX, EditMode.BOX_SECOND_SHINY),
        (prd.shiny.box.third, RenderTarget.BOX, EditMode.BOX_THIRD_SHINY)
    ]:
        if render_data.main_camera is None:
            render_data.main_camera = get_default_camera(render_target, mode, prd)


def get_default_camera(render_target: RenderTarget, mode: EditMode, prd: PokemonRenderData) -> Camera:
    scene_camera = bpy.data.objects["PKXIconGen_Camera"]
    scene_focus_point = bpy.data.objects["PKXIconGen_FocusPoint"]

    armature_obj = get_armature_obj(prd, mode)
    armature = armature_obj.data
    head_bone = get_head_bone(armature)
    head_pose_bone = armature_obj.pose.bones[head_bone.name]
    parent_bone = head_bone.parent
    parent_pose_bone = armature_obj.pose.bones[parent_bone.name]

    # Position focus point
    head_loc = armature_obj.matrix_world @ head_pose_bone.matrix @ head_pose_bone.location
    parent_loc = armature_obj.matrix_world @ parent_pose_bone.matrix @ parent_pose_bone.location
    focus_loc = Vector3(head_loc.x, parent_loc.y, head_loc.z)
    scene_focus_point.location = focus_loc.to_mathutils_vector()

    # Camera pos, based on model dimensions
    armature_dimensions = armature_obj.dimensions
    distance = math.sqrt(max(armature_dimensions.x, armature_dimensions.y, armature_dimensions.z) * 2)
    camera_loc = Vector3(distance, -distance, head_loc.z)
    scene_camera.location = camera_loc.to_mathutils_vector()

    # Instead of direct parent, go back until any mesh is found?
    objects_to_focus = find_armature_objects_affected_by_bone(armature_obj, parent_bone)
    common.focus_camera_on_objects(objects_to_focus)

    return Camera(camera_loc, focus_loc, True, round(math.degrees(scene_camera.data.angle)), scene_camera.data.ortho_scale, Light.default(render_target))

def get_head_bone(armature):
    for bone in armature.bones:
        if bone.name.endswith("_Head"):
            return bone

    raise Exception("Head bone not found")


# Based on vertex group names
def find_armature_objects_affected_by_bone(armature_obj, bone):
    meshes_objs = list()
    for child in armature_obj.children:
        vertex_groups = child.vertex_groups
        for vertex_group in vertex_groups:
            if vertex_group.name == bone.name:
                meshes_objs.append(child)
                break

    return meshes_objs