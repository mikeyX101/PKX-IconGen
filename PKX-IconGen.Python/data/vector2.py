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

from math import radians
from typing import Optional
# noinspection PyUnresolvedReferences
from mathutils import Euler
# noinspection PyUnresolvedReferences
from mathutils import Vector as MUVector


class Vector2(object):
    
    def __init__(self, x: float, y: float):
        self.x = x
        self.y = y

    def to_mathutils_vector(self) -> MUVector:
        return MUVector((
            self.x,
            self.y,
        ))

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Vector2']:
        if obj is None:
            return None

        return Vector2(
            obj.x,
            obj.y
        )
