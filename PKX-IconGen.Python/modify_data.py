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
from typing import Optional

sys.path.append(os.getcwd())

from data.pokemon_render_data import PokemonRenderData
import utils
from addon import register

if __name__ == "__main__":
    debug_json: Optional[str] = None
    prd: PokemonRenderData

    args = utils.parse_cmd_args()
    for arg, value in args:
        if arg == "--pkx-debug":
            debug_json = value

    if debug_json is not None:
        file = open(debug_json, "r")
        json = file.readline()
        file.close()

        prd = PokemonRenderData.from_json(json)
    else:
        json = sys.stdin.readline()
        print(json)
        prd = PokemonRenderData.from_json(json)

    utils.import_model(prd.render.model, prd.shiny.hue)
    register(prd)
