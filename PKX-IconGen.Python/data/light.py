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

from enum import IntEnum
from typing import Optional

from .color import Color
from .render_target import RenderTarget


class LightType(IntEnum):
    POINT = 0
    SUN = 1
    SPOT = 2
    AREA = 3


class Light(object):
    
    def __init__(self, type: LightType, strength: float, color: Color, distance: float):
        self.type = type
        self.strength = strength
        self.color = color
        self.distance = distance

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Light']:
        if obj is None:
            return None

        return Light(
            LightType(obj.type),
            obj.strength,
            Color.parse_obj(obj.color),
            obj.distance)

    @staticmethod
    def default(target: RenderTarget) -> 'Light':
        light: Light
        if target is RenderTarget.FACE:
            light = Light(
                LightType.POINT,
                250,
                Color.default(),
                5
            )
        elif target is RenderTarget.BOX:
            light = Light(
                LightType.AREA,
                650,
                Color.default(),
                13
            )
        else:
            raise Exception(f"Unknown RenderTarget: {target.name}")

        return light
