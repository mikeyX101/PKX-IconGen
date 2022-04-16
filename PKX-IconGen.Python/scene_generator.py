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
import mathutils
from math import radians
from data.pokemon_render_data import PokemonRenderData
from importer import import_hsd

def generate_scene(data: PokemonRenderData):
    main_render = data.render
    import_hsd.load(None, bpy.context, main_render.model, 0, "scene_data", "SCENE", True, True, 1000, True)

    objs = bpy.data.objects

    # Move camera
    camera = objs["PKXIconGen_Camera"]
    camera.location = main_render.main_camera.pos.to_mathutils_vector()
    camera.rotation_euler = main_render.main_camera.rot.to_mathutils_euler()

    camera.data.lens_unit = "FOV"
    camera.data.angle = radians(main_render.main_camera.fov)

    #TODO Move to addon
    

    # Remove bounding boxes cube, normally the first 7 objects
    #objs.remove(objs["Object"])
    #for i in range(1, 7):
    #    objs.remove(objs["Object.00" + str(i)])