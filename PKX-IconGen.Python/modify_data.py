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
from data.pokemon_render_data import PokemonRenderData
import scene_generator
from addon import register

data: PokemonRenderData = PokemonRenderData.from_json(sys.stdin.readline())
scene_generator.generate_scene(data)

register(data)

bpy.ops.wm.show_region_ui()