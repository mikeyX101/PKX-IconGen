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
from typing import Optional, List

from .edit_mode import EditMode
from .camera import Camera
from .render_data import RenderData
from .shiny_info import ShinyInfo


class PokemonRenderData(object):

    def __init__(self,
                 name: str,
                 output_name: Optional[str],
                 render: RenderData,
                 shiny: ShinyInfo):
        self.name = name
        self.output_name = output_name
        self.render = render
        self.shiny = shiny

    def to_json(self) -> str:
        return json.dumps(self, default=vars, separators=(',', ':'))

    def get_mode_model(self, mode: EditMode) -> RenderData:
        if mode == EditMode.NORMAL or mode == EditMode.NORMAL_SECONDARY:
            return self.render.model
        elif mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            if self.shiny.render.model is not None and self.shiny.render.model != "":
                return self.shiny.render.model
            else:
                return self.render.model
        else:
            raise Exception("Unknown edit mode: " + mode.name)

    def get_mode_render(self, mode: EditMode) -> RenderData:
        if mode == EditMode.NORMAL or mode == EditMode.NORMAL_SECONDARY:
            return self.render
        elif mode == EditMode.SHINY or mode == EditMode.SHINY_SECONDARY:
            return self.shiny.render
        else:
            raise Exception("Unknown edit mode: " + mode.name)

    def get_mode_camera(self, mode: EditMode) -> Optional[Camera]:
        render: RenderData = self.get_mode_render(mode)
        if mode == EditMode.NORMAL or mode == EditMode.SHINY:
            return render.main_camera
        elif mode == EditMode.NORMAL_SECONDARY or mode == EditMode.SHINY_SECONDARY:
            return render.secondary_camera
        else:
            raise Exception("Unknown edit mode: " + mode.name)

    def get_mode_animation_pose(self, mode: EditMode) -> int:
        render: RenderData = self.get_mode_render(mode)
        return render.animation_pose

    def get_mode_animation_frame(self, mode: EditMode) -> int:
        render: RenderData = self.get_mode_render(mode)
        return render.animation_frame

    def get_mode_removed_objects(self, mode: EditMode) -> List[str]:
        render: RenderData = self.get_mode_render(mode)
        return render.removed_objects

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['PokemonRenderData']:
        if obj is None:
            return obj

        output_name: Optional[str] = None
        try:
            output_name = obj.output_name
        except AttributeError:
            pass

        return PokemonRenderData(
            obj.name,
            output_name,
            RenderData.parse_obj(obj.render),
            ShinyInfo.parse_obj(obj.shiny))

    @staticmethod
    def from_json(json_str: str) -> Optional['PokemonRenderData']:
        prd = json.loads(json_str, object_hook=lambda d: SimpleNamespace(**d))

        return PokemonRenderData.parse_obj(prd)
