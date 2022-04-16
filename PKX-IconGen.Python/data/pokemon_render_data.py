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

import json
from types import SimpleNamespace
from typing import List
from typing import Optional

from .render_data import RenderData
from .shiny_info import ShinyInfo

class PokemonRenderData(object):

    def __init__(self,
                 name: str,
                 output_name: str,
                 render: RenderData,
                 shiny: ShinyInfo,
                 removed_objects: List[str]):

        self.name = name
        self.output_name = output_name
        self.render = render
        self.shiny = shiny
        self.removed_objects = removed_objects

    def to_json(self) -> str:
        return json.dumps(self, default=vars, separators=(',', ':'))

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['PokemonRenderData']:
        if obj is None:
            return obj

        return PokemonRenderData(
            obj.name,
            obj.output_name,
            RenderData.parse_obj(obj.render),
            ShinyInfo.parse_obj(obj.shiny),
            obj.removed_objects)

    @staticmethod
    def from_json(json_str: str):
        prd = json.loads(json_str, object_hook=lambda d: SimpleNamespace(**d))

        return PokemonRenderData.parse_obj(prd)