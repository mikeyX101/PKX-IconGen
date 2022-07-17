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

from .vector2 import Vector2


class Texture(object):

    def __init__(self, texture_name: str, image_path: Optional[str], mapping: dict[str, Vector2]) -> 'Texture':
        self.texture_name = texture_name
        self.image_path = image_path
        self.mapping = mapping

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Texture']:
        if obj is None:
            return obj

        image_path: Optional[str] = None
        if "image_path" in obj.__dict__.keys():
            image_path = obj.image_path

        mapping_dict: dict[str, Vector2] = dict()
        for mat in obj.mapping:
            mapping_dict[mat] = Vector2.parse_obj(obj.mapping[mat])

        return Texture(
            obj.texture_name,
            image_path,
            mapping_dict
        )
