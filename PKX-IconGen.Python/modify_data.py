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
import common
from addon import register

if __name__ == "__main__":
    debug_json: Optional[str] = None
    debug_egg: Optional[str] = None
    prd: PokemonRenderData

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

        common.print_verbose(f"Input: {json}")
        prd = PokemonRenderData.from_json(json)
    else:
        json = sys.stdin.readline()

        common.print_verbose(f"Input: {json}")
        prd = PokemonRenderData.from_json(json)

    common.import_models(prd)
    register(prd)
