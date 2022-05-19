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
from typing import Optional, Sequence, List

# noinspection PyUnresolvedReferences
from mathutils import Vector as MUVector


class Color(object):
    
    def __init__(self, r: float, g: float, b: float):
        self.r = r
        self.b = b
        self.g = g

    def to_list(self) -> List[float]:
        return [
            self.r,
            self.g,
            self.b
        ]

    def to_list_alpha(self) -> List[float]:
        return [
            self.r,
            self.g,
            self.b,
            1
        ]

    def to_mathutils_vector(self) -> MUVector:
        return MUVector((
            self.r,
            self.g,
            self.b
        ))

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Color']:
        if obj is None:
            return None

        return Color(
            obj.r,
            obj.g,
            obj.b
        )

    @staticmethod
    def default() -> 'Color':
        return Color(
            1,
            1,
            1
        )
