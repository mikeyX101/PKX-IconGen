""" License
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2023 Samuel Caron/mikeyx

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

from enum import IntEnum
from typing import Optional

from .render_data import RenderData


class BoxAnimationFrame(IntEnum):
    FIRST = 0,
    SECOND = 1,
    THIRD = 2


class BoxInfo(object):

    def __init__(self, first: RenderData, second: RenderData, third: RenderData):
        self.first = first
        self.second = second
        self.third = third

    def get_box_render_data(self, frame: BoxAnimationFrame):
        render: RenderData
        if frame == BoxAnimationFrame.FIRST:
            render = self.first
        elif frame == BoxAnimationFrame.SECOND:
            render = self.second
        elif frame == BoxAnimationFrame.THIRD:
            render = self.third
        else:
            raise Exception("Got an unknown BoxAnimationFrame: " + frame.name)
        return render

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['BoxInfo']:
        if obj is None:
            return obj

        return BoxInfo(
            RenderData.parse_obj(obj.first),
            RenderData.parse_obj(obj.second),
            RenderData.parse_obj(obj.third)
        )
