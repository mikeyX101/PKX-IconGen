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
from blender_data_enums import *
from math import radians
from typing import Optional

import blender_compat
import bpy
import bmesh
import os


"""
This file contains manual patches made for individual Pokemon to fix important broken features from models like mapping 
issues or tweaking values in shader nodes.

To gradually remove after improvements are made to the importer
Patches with a !! means the patch fixes issues, otherwise it's just specific visual improvements
"""


def apply_patches_by_model_name(model_path: Optional[str]):
    if model_path is None:
        return

    model_name: str = os.path.basename(model_path)

    if "asanan.pkx" in model_name:  # !!Meditite, fix eyes map and set scale X to 1
        _flip_outside_uv_x("Object.013")
        _set_mat_map_data("Material.005", "Mapping", blender_compat.mapping_in.scale, 1)
        print("Patched !!Meditite")
    elif "donmel.pkx" in model_name:  # !!Numel, fix volcano map
        _set_mat_map_data("Material.007", "Mapping", blender_compat.mapping_in.location, 0.23, 0.13)
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
        _make_obj_track_camera("Object.002")
        _make_obj_track_camera("Object.003")
        print("Patched Ghastly")
    elif "achamo.pkx" == model_name or "achamo.pkx.dat" == model_name:  # !!Torchic, fix eyes map
        _set_mat_map_data("Material.007", "Mapping", blender_compat.mapping_in.location, 0, -9)
        print("Patched Torchic")
    elif "gomazou.pkx" in model_name:  # !!Phanpy, fix nose map
        _set_mat_map_data("Material.005", "Mapping", blender_compat.mapping_in.location, 0, -0.11)
        _set_mat_map_data("Material.005", "Mapping", blender_compat.mapping_in.scale, 0.55, 1.5)
        _set_mat_texture_ext_mode("Material.005", "0x2D50 flag: 30010 image: 0x2FC60  tlut: 0x3CE00", "MIRROR")
        print("Patched Phanpy")
    elif "anopth.pkx" in model_name:  # Anorith, make eyes track camera like a sprite and fix alpha blending
        _make_obj_track_camera("Object.002")
        _make_obj_track_camera("Object.004")
        _set_mat_blend_mode("Material.006", BlendMethod.BLEND)
        _set_mat_blend_mode("Material", BlendMethod.BLEND)
        print("Patched Anorith")
    elif "tutinin.pkx" in model_name:  # Nincada, fix wings alpha blending
        _set_mat_blend_mode("Material.008", BlendMethod.BLEND)
        _set_mat_shadow_mode("Material.008", ShadowMethod.HASHED)
        _set_mat_blend_mode("Material.013", BlendMethod.BLEND)
        _set_mat_shadow_mode("Material.013", ShadowMethod.HASHED)
        print("Patched Nincada")
    elif "pearlulu.pkx" in model_name:  # Clamperl, fix pearl reflection to be less busy looking and fix face alpha blending
        _set_tex_projection("Material.004", "0x3F60 flag: 30081 image: 0x130A0  tlut: 0x-1", TextureProjection.SPHERE)
        _set_mat_map_data("Material.004", "Mapping.001", blender_compat.mapping_in.location, -2, 2, 0)
        _set_mat_map_data("Material.004", "Mapping.001", blender_compat.mapping_in.rotation, 95, -95, 80)
        _set_mat_blend_mode("Material", BlendMethod.BLEND)
        _set_mat_shadow_mode("Material", ShadowMethod.NONE)
        _set_mat_blend_mode("Material.002", BlendMethod.BLEND)
        _set_mat_shadow_mode("Material.002", ShadowMethod.NONE)
        _set_mat_blend_mode("Material.003", BlendMethod.BLEND)
        _set_mat_shadow_mode("Material.003", ShadowMethod.NONE)
        print("Patched Clamperl")
    elif "fushigidane.pkx" in model_name:  # Bulbasaur, fix vertex on nose by dissolving it
        _dissolve_vertex("Object.013", 5)
        print("Patched Bulbasaur")


def _start_mesh_edit(obj_name: str) -> bmesh.types.BMesh:
    view_layer = bpy.context.view_layer
    obj = bpy.data.objects[obj_name]
    mesh = obj.data

    for obj in bpy.context.selected_objects:
        obj.select_set(False)
    obj.select_set(True)
    view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    return bmesh.from_edit_mesh(mesh)


def _stop_mesh_edit(obj_name: str):
    obj = bpy.data.objects[obj_name]
    mesh = obj.data

    bmesh.update_edit_mesh(mesh)
    bpy.ops.object.mode_set(mode="OBJECT")
    for obj in bpy.context.selected_objects:
        obj.select_set(False)


def _flip_outside_uv_x(obj_name: str):
    bm = _start_mesh_edit(obj_name)

    uv_layer = bm.loops.layers.uv.verify()
    for face in bm.faces:
        for loop in face.loops:
            loop_uv = loop[uv_layer]
            if loop_uv.uv[0] > 1:
                flip_offset = loop_uv.uv[0] - 1
                loop_uv.uv[0] = loop_uv.uv[0] - (flip_offset * 2)

    _stop_mesh_edit(obj_name)


def _set_mat_bump_strength_to(mat_name: str, bump_name: str, strength: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        tree.nodes[bump_name].inputs[blender_compat.bump_in.strength].default_value = strength


def _set_mat_mix_factor_to(mat_name: str, mix_name: str, fac: float):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        tree.nodes[mix_name].inputs[blender_compat.mix_in.factor].default_value = fac


def _set_mat_map_data(mat_name: str, map_name: str, data_idx: int, x: Optional[float] = None, y: Optional[float] = None, z: Optional[float] = None):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    modifying_rotation = data_idx == blender_compat.mapping_in.rotation
    if tree is not None:
        xyz = tree.nodes[map_name].inputs[data_idx].default_value
        if x is not None:
            xyz[0] = radians(x) if modifying_rotation else x
        if y is not None:
            xyz[1] = radians(y) if modifying_rotation else y
        if z is not None:
            xyz[2] = radians(z) if modifying_rotation else z


def _set_mat_texture_ext_mode(mat_name: str, tex_node: str, ext_mode: str):
    mat = bpy.data.materials[mat_name]
    tree = mat.node_tree
    if tree is not None:
        tree.nodes[tex_node].extension = ext_mode


def _make_obj_track_camera(obj_name: str):
    view_layer = bpy.context.view_layer
    camera = bpy.data.objects['PKXIconGen_Camera']

    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    aura_obj = bpy.data.objects[obj_name]
    aura_obj.select_set(True)
    view_layer.objects.active = aura_obj
    bpy.ops.object.constraint_add(type='TRACK_TO')
    track_constraint = aura_obj.constraints['Track To']
    track_constraint.target = camera
    track_constraint.track_axis = 'TRACK_Z'


def _set_mat_blend_mode(mat_name: str, blend_mode: BlendMethod):
    bpy.data.materials[mat_name].blend_method = blend_mode.name


def _set_mat_shadow_mode(mat_name: str, shadow_mode: ShadowMethod):
    bpy.data.materials[mat_name].shadow_method = shadow_mode.name


def _set_bsdf_roughness(mat_name: str, bsdf_roughness: float):
    bpy.data.materials[mat_name].node_tree.nodes["Principled BSDF"].inputs[blender_compat.principled_bsdf_in.roughness].default_value = bsdf_roughness


def _set_tex_projection(mat_name: str, tex_node: str, projection: TextureProjection):
    bpy.data.materials[mat_name].node_tree.nodes[tex_node].projection = projection.name


def _dissolve_vertex(obj_name: str, vertex_idx: int):
    bm = _start_mesh_edit(obj_name)

    bm.verts.ensure_lookup_table()
    for vert in bm.verts:
        vert.select_set(False)
    bm.verts[vertex_idx].select_set(True)
    bpy.ops.mesh.dissolve_verts()

    _stop_mesh_edit(obj_name)
