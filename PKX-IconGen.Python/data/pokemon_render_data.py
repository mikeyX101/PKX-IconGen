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
from typing import Optional, List, Callable

from .box_info import BoxInfo
from .edit_mode import EditMode
from .camera import Camera
from .object_shading import ObjectShading
from .render_data import RenderData
from .shiny_info import ShinyInfo
from .texture import Texture


class PokemonRenderData(object):

    def __init__(self,
                 name: str,
                 output_name: Optional[str],
                 model: str,
                 face: RenderData,
                 box: BoxInfo,
                 shiny: ShinyInfo):
        self.name = name
        self.output_name = output_name
        self.model = model
        self.face = face
        self.box = box
        self.shiny = shiny

    def to_json(self) -> str:
        return json.dumps(self, default=vars, separators=(',', ':'))

    def get_mode_model(self, mode: EditMode) -> str:
        if mode in EditMode.ANY_NORMAL:
            return self.model
        elif mode in EditMode.ANY_SHINY:
            return self.shiny.model if self.shiny.model is not None else self.model
        else:
            raise Exception(f"Unknown edit mode: {mode.name}")

    def get_mode_render(self, mode: EditMode) -> RenderData:
        if mode in EditMode.ANY_FACE_NORMAL:
            return self.face
        elif mode in EditMode.ANY_FACE_SHINY:
            return self.shiny.face
        elif mode == EditMode.BOX_FIRST:
            return self.box.first
        elif mode == EditMode.BOX_FIRST_SHINY:
            return self.shiny.box.first
        elif mode == EditMode.BOX_SECOND:
            return self.box.second
        elif mode == EditMode.BOX_SECOND_SHINY:
            return self.shiny.box.second
        elif mode == EditMode.BOX_THIRD:
            return self.box.third
        elif mode == EditMode.BOX_THIRD_SHINY:
            return self.shiny.box.third
        else:
            raise Exception(f"Unknown edit mode: {mode.name}")

    def update_mode_render(self, mode: EditMode, update_func: Callable[[RenderData], None]):
        render: RenderData = self.get_mode_render(mode)
        update_func(render)

    def get_mode_camera(self, mode: EditMode) -> Optional[Camera]:
        render: RenderData = self.get_mode_render(mode)
        if mode in EditMode.ANY_FACE_MAIN or mode in EditMode.ANY_BOX:
            return render.main_camera
        elif mode in EditMode.ANY_FACE_SECONDARY:
            return render.secondary_camera
        else:
            raise Exception(f"Unknown edit mode: {mode.name}")

    def get_mode_animation_pose(self, mode: EditMode) -> int:
        render: RenderData = self.get_mode_render(mode)
        return render.animation_pose

    def get_mode_animation_frame(self, mode: EditMode) -> int:
        render: RenderData = self.get_mode_render(mode)
        return render.animation_frame

    def get_mode_removed_objects(self, mode: EditMode) -> List[str]:
        render: RenderData = self.get_mode_render(mode)
        return render.removed_objects

    def get_mode_textures(self, mode: EditMode) -> List[Texture]:
        render: RenderData = self.get_mode_render(mode)
        return render.textures

    def get_mode_shading(self, mode: EditMode) -> ObjectShading:
        render: RenderData = self.get_mode_render(mode)
        return render.shading

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['PokemonRenderData']:
        if obj is None:
            return obj

        output_name: Optional[str] = None
        if "output_name" in obj.__dict__.keys():
            output_name = obj.output_name

        return PokemonRenderData(
            obj.name,
            output_name,
            obj.model,
            RenderData.parse_obj(obj.face),
            BoxInfo.parse_obj(obj.box),
            ShinyInfo.parse_obj(obj.shiny))

    @staticmethod
    def from_json(json_str: str) -> Optional['PokemonRenderData']:
        prd = json.loads(json_str, object_hook=lambda d: SimpleNamespace(**d))

        return PokemonRenderData.parse_obj(prd)
