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

from typing import Optional

from .light import Light
from .vector3 import Vector3


class Camera(object):
    
    def __init__(self,
                 pos: Vector3,
                 focus: Vector3,
                 fov: float,
                 light: Light):
        self.pos = pos
        self.focus = focus
        self.fov = fov
        self.light = light

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Camera']:
        if obj is None:
            return None

        return Camera(
            Vector3.parse_obj(obj.pos),
            Vector3.parse_obj(obj.focus),
            obj.fov,
            Light.parse_obj(obj.light))

    @staticmethod
    def default() -> 'Camera':
        return Camera(
            Vector3(14, -13.5, 5.5),
            Vector3(0, 0, 0),
            40,
            Light.default())
