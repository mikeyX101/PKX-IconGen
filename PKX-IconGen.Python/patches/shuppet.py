""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2024 Samuel Caron/mikeyX#4697

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

import blender_compat

import bpy


# Fix body material being multiplied by unknown color attribute
def patch():
    mat = bpy.data.materials["Material.009"]
    tree = mat.node_tree
    if tree is not None:
        bsdf = tree.nodes["Principled BSDF"]
        mix_mult_node = bsdf.inputs[blender_compat.principled_bsdf_in.base_color].links[0].from_node
        if mix_mult_node.bl_idname == "ShaderNodeMixRGB" and mix_mult_node.blend_type == 'MULTIPLY':
            color_map_blend_node = mix_mult_node.inputs[blender_compat.mix_in.color1].links[0].from_node
            if color_map_blend_node.bl_idname == "ShaderNodeMixRGB" and color_map_blend_node.blend_type == 'MIX':
                tree.links.new(color_map_blend_node.outputs[0], bsdf.inputs[blender_compat.principled_bsdf_in.base_color])
