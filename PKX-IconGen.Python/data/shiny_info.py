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

from .render_data import RenderData


class ShinyInfo(object):
    
    def __init__(self, hue: Optional[float], render: RenderData):
        self.hue = hue
        self.render = render

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['ShinyInfo']:
        if obj is None:
            return obj

        hue: Optional[float] = None
        if "hue" in obj.__dict__.keys():
            hue = obj.hue

        return ShinyInfo(
            hue,
            RenderData.parse_obj(obj.render)
        )
