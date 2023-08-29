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

from .game import Game
from .render_target import RenderTarget
from .pokemon_render_data import PokemonRenderData


class RenderJob(object):

    def __init__(self,
                 data: PokemonRenderData,
                 scale: int,
                 game: Game,
                 target: RenderTarget,
                 face_main_path: str,
                 face_shiny_path: str,
                 face_secondary_path: str,
                 face_shiny_secondary_path: str,
                 box_first_main_path: str,
                 box_first_shiny_path: str,
                 box_second_main_path: str,
                 box_second_shiny_path: str,
                 box_third_main_path: str,
                 box_third_shiny_path: str):
        self.data = data
        self.scale = scale
        self.game = game
        self.target = target
        self.face_main_path = face_main_path
        self.face_shiny_path = face_shiny_path
        self.face_secondary_path = face_secondary_path
        self.face_shiny_secondary_path = face_shiny_secondary_path
        self.box_first_main_path = box_first_main_path
        self.box_first_shiny_path = box_first_shiny_path
        self.box_second_main_path = box_second_main_path
        self.box_second_shiny_path = box_second_shiny_path
        self.box_third_main_path = box_third_main_path
        self.box_third_shiny_path = box_third_shiny_path

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['RenderJob']:
        if obj is None:
            return obj

        return RenderJob(
            PokemonRenderData.parse_obj(obj.data),
            obj.scale,
            Game(obj.game),
            RenderTarget(obj.target),
            obj.face_main_path,
            obj.face_shiny_path,
            obj.face_secondary_path,
            obj.face_shiny_secondary_path,
            obj.box_first_main_path,
            obj.box_first_shiny_path,
            obj.box_second_main_path,
            obj.box_second_shiny_path,
            obj.box_third_main_path,
            obj.box_third_shiny_path
        )

    @staticmethod
    def from_json(json_str: str) -> Optional['RenderJob']:
        job = json.loads(json_str, object_hook=lambda d: SimpleNamespace(**d))

        return RenderJob.parse_obj(job)
