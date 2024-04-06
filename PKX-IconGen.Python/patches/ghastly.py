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


# Make aura track camera (like a sprite)
def patch():
    view_layer = bpy.context.view_layer
    camera = bpy.data.objects['PKXIconGen_Camera']
    track_for = ["Object.002", "Object.003"]
    for aura_obj_name in track_for:
        for obj in bpy.context.selected_objects:
            obj.select_set(False)

        aura_obj = bpy.data.objects[aura_obj_name]
        aura_obj.select_set(True)
        view_layer.objects.active = aura_obj
        bpy.ops.object.constraint_add(type='TRACK_TO')
        track_constraint = aura_obj.constraints['Track To']
        track_constraint.target = camera
        track_constraint.track_axis = 'TRACK_Z'
