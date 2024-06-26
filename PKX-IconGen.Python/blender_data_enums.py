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
from enum import Enum


class BlendMethod(Enum):
    OPAQUE = 0,
    CLIP = 1,
    HASHED = 2,
    BLEND = 3


class ShadowMethod(Enum):
    NONE = 0,
    OPAQUE = 1,
    CLIP = 2,
    HASHED = 3


class TextureProjection(Enum):
    FLAT = 0,
    BOX = 1,
    SPHERE = 2,
    TUBE = 3
