""" License 
    PKX-IconGen.Python.Core - Core Python classes for PKX-IconGen to interact with Blender
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
from enum import Enum

from data.color import Color
from data.vector import Vector

class LightType(Enum):
    POINT = 0
    SUN = 1
    SPOT = 2
    AREA = 3

class Light(object):
    
    def __init__(self, pos: Vector, rot: Vector, light_type: LightType, strength: float, color: Color):

        self.pos = pos
        self.rot = rot
        self.light_type = light_type
        self.strength = strength
        self.color = color


