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
from typing import Optional

from .pokemon_render_data import PokemonRenderData


class RenderJob(object):

    def __init__(self,
                 data: PokemonRenderData,
                 scale: int,
                 game: int,
                 main_path: str,
                 shiny_path: str,
                 secondary_path: str,
                 shiny_secondary_path: str):
        self.data = data
        self.scale = scale
        self.game = game
        self.main_path = main_path
        self.shiny_path = shiny_path
        self.secondary_path = secondary_path
        self.shiny_secondary_path = shiny_secondary_path

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['RenderJob']:
        if obj is None:
            return obj

        return RenderJob(
            PokemonRenderData.parse_obj(obj.data),
            obj.scale,
            obj.game,
            obj.main_path,
            obj.shiny_path,
            obj.secondary_path,
            obj.shiny_secondary_path)

    @staticmethod
    def from_json(json_str: str) -> Optional['RenderJob']:
        job = json.loads(json_str, object_hook=lambda d: SimpleNamespace(**d))

        return RenderJob.parse_obj(job)
