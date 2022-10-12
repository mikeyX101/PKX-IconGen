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
from enum import Enum,IntEnum
from typing import Optional


class ShinyColor(object):

    def __init__(self, r: int, g: int, b: int, a: int):
        self.r = r
        self.b = b
        self.g = g
        self.a = a

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['ShinyColor']:
        if obj is None:
            return None

        return ShinyColor(
            obj.r,
            obj.g,
            obj.b,
            obj.a
        )

    @staticmethod
    def default_color1() -> 'ShinyColor':
        return ShinyColor(
            0,
            1,
            2,
            3
        )

    @staticmethod
    def default_color2() -> 'ShinyColor':
        return ShinyColor(
            0x7F,
            0x7F,
            0x7F,
            0x7F
        )


class ShinyColors(Enum):
    Color1 = 0
    Color2 = 1


# Values are node input index
class ColorChannel(IntEnum):
    R = 2
    G = 3
    B = 4
    A = 5
