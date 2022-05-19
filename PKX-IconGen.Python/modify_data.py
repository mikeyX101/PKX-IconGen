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

import sys
import os

sys.path.append(os.getcwd())

from data.pokemon_render_data import PokemonRenderData
import scene_generator
from addon import register

debug_flag: bool = False
prd: PokemonRenderData

for arg in sys.argv:
    if arg == "--pkx-debug":
        debug_flag = True
        break


if debug_flag:
    prd = PokemonRenderData.from_json(
        "{\"name\":\"Ludicolo\",\"output_name\":\"runpappa\","
        
        "\"render\":"
        "{\"model\":\"" + os.getcwd() + "/debugging/runpappa.pkx.dat\","
        "\"animation_pose\":0,\"animation_frame\":0,"
        "\"main_camera\":{"
        "\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},"
        "\"focus\":{\"x\":0,\"y\":0,\"z\":0},"
        "\"fov\":40,"
        "\"light\":{\"type\":3,\"strength\":130,\"color\":{\"r\":1,\"g\":1,\"b\":1},\"distance\":5}},"
        "\"removed_objects\":[]},"
        
        "\"shiny\":"
        "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},"
        "\"render\":"
        "{\"model\":\"\","
        "\"animation_pose\":0,\"animation_frame\":0,"
        "\"main_camera\":{"
        "\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},"
        "\"focus\":{\"x\":0,\"y\":0,\"z\":10},"
        "\"fov\":40,"
        "\"light\":{\"type\":3,\"strength\":130,\"color\":{\"r\":1,\"g\":1,\"b\":1},\"distance\":5}},"
        "\"removed_objects\":[]}}"
        "}"
    )
else:
    prd: PokemonRenderData = PokemonRenderData.from_json(sys.stdin.readline())

scene_generator.generate_scene(prd)
register(prd)
