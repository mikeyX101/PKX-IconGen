""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2024 Samuel Caron/mikeyx

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

from enum import Flag


class DataType(Flag):
    NONE = 0
    ANIMATION = 1
    CAMERA_LIGHT = 2
    SHADING = 4
    REMOVED_OBJECTS = 8
    TEXTURES = 16

    @staticmethod
    def from_blender_flags(blender_flags: set[str]) -> 'DataType':
        types: DataType = DataType.NONE
        if "REMOVED_OBJECTS" in blender_flags:
            types |= DataType.REMOVED_OBJECTS
        if "ANIMATION" in blender_flags:
            types |= DataType.ANIMATION
        if "CAMERA_LIGHT" in blender_flags:
            types |= DataType.CAMERA_LIGHT
        if "TEXTURES" in blender_flags:
            types |= DataType.TEXTURES
        if "SHADING" in blender_flags:
            types |= DataType.SHADING
        return types

