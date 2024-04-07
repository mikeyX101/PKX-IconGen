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
from typing import Optional

import blender_compat
import bpy
import bmesh
import os

"""
This file contains manual patches made for individual Pokemon to fix important broken features from models like mapping 
issues or tweaking values in shader nodes.

To gradually remove after improvements are made to the importer
Patches with a !! means the patch fixes issues, otherwise it's just specific improvements
"""


def apply_patches_by_model_name(model_path: Optional[str]):
    if model_path is None:
        return

    model_name: str = os.path.basename(model_path)

    if "asanan.pkx" in model_name:  # !!Meditite, fix eyes map and set scale X to 1
        _flip_outside_uv_x("Object.013")
        _set_mat_map_input_to("Material.005", "Mapping", blender_compat.mapping_in.scale, 1)
        print("Patched !!Meditite")
    elif "donmel.pkx" in model_name:  # !!Numel, fix volcano map
        _set_mat_map_loc_to("Material.007", "Mapping", 0.23, 0.13)
        print("Patched !!Numel")
    elif "taneboh.pkx" in model_name:  # Seedot, put more strength in bump
        _set_mat_bump_strength_to("Material.003", "Bump", 0.2)
        print("Patched Seedot")
    elif "dumbber.pkx" in model_name:  # Beldum, eye appears desaturated
        _set_mat_mix_factor_to("Material.001", "TEX_COLORMAP_BLEND 0.8999999761581421", 1)
        print("Patched Beldum")
    elif "kagebouzu.pkx" in model_name:  # !!Shuppet, fix body colors
        from patches import shuppet
        shuppet.patch()
        print("Patched !!Shuppet")
    elif "ghos.pkx" in model_name:  # Ghastly, make aura track camera like a sprite
        from patches import ghastly
        ghastly.patch()
        print("Patched Ghastly")
    elif "achamo.pkx" == model_name or "achamo.pkx.dat" == model_name:  # !!Torchic, fix eyes map
        _set_mat_map_loc_to("Material.007", "Mapping", 0, -9)
        print("Patched Torchic")


def _flip_outside_uv_x(obj_name: str):
    obj = bpy.data.objects[obj_name]
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    me = obj.data
    bm = bmesh.from_edit_mesh(me)

    uv_layer = bm.loops.layers.uv.verify()
    for face in bm.faces:
        for loop in face.loops:
            loop_uv = loop[uv_layer]
            if loop_uv.uv[0] > 1:
                flip_offset = loop_uv.uv[0] - 1
                loop_uv.uv[0] = loop_uv.uv[0] - (flip_offset * 2)

    bmesh.update_edit_mesh(me)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.select_set(False)


def _set_mat_map_input_to(mat_name: str, mapnode_name: str, input_index: int, value: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        xyz = tree.nodes[mapnode_name].inputs[input_index].default_value
        xyz[0] = xyz[1] = xyz[2] = value


def _set_mat_bump_strength_to(mat_name: str, bumpnode_name: str, strength: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        tree.nodes[bumpnode_name].inputs[blender_compat.bump_in.strength].default_value = strength


def _set_mat_mix_factor_to(mat_name: str, mixnode_name: str, fac: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        tree.nodes[mixnode_name].inputs[blender_compat.mix_in.factor].default_value = fac


def _set_mat_map_loc_to(mat_name: str, mapnode_name: str, x: float, y: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        xyz = tree.nodes[mapnode_name].inputs[blender_compat.mapping_in.location].default_value
        xyz[0] = x
        xyz[1] = y
