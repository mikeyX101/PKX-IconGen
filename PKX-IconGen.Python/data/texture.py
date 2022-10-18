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

from .material import Material


class Texture(object):

    def __init__(self, name: str, path: Optional[str], mats: Optional[list[Material]]):
        self.name = name
        self.path = path
        self.mats = mats or list()

    def get_material_by_name(self, name: str) -> Optional[Material]:
        for mat in self.mats:
            if mat.name == name:
                return mat

        return None

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['Texture']:
        if obj is None:
            return obj

        image_path: Optional[str] = None
        if "path" in obj.__dict__.keys():
            image_path = obj.path

        mats: list[Material] = list[Material]()
        if "mats" in obj.__dict__.keys():
            for material in obj.mats:
                mats.append(Material.parse_obj(material))

        return Texture(
            obj.name,
            image_path,
            mats
        )
