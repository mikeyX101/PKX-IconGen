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

from enum import Flag


class EditMode(Flag):
    FACE_NORMAL = 1
    FACE_NORMAL_SECONDARY = 2
    FACE_SHINY = 4
    FACE_SHINY_SECONDARY = 8

    BOX_FIRST = 16
    BOX_FIRST_SHINY = 32
    BOX_SECOND = 64
    BOX_SECOND_SHINY = 128
    BOX_THIRD = 256
    BOX_THIRD_SHINY = 512

    ANY_FACE = FACE_NORMAL | FACE_NORMAL_SECONDARY | FACE_SHINY | FACE_SHINY_SECONDARY
    ANY_FACE_MAIN = FACE_NORMAL | FACE_SHINY
    ANY_FACE_NORMAL = FACE_NORMAL | FACE_NORMAL_SECONDARY
    ANY_FACE_SHINY = FACE_SHINY | FACE_SHINY_SECONDARY
    ANY_FACE_SECONDARY = FACE_NORMAL_SECONDARY | FACE_SHINY_SECONDARY

    ANY_BOX = BOX_FIRST | BOX_FIRST_SHINY | BOX_SECOND | BOX_SECOND_SHINY | BOX_THIRD | BOX_THIRD_SHINY
    ANY_BOX_FIRST = BOX_FIRST | BOX_FIRST_SHINY
    ANY_BOX_SECOND = BOX_SECOND | BOX_SECOND_SHINY
    ANY_BOX_THIRD = BOX_THIRD | BOX_THIRD_SHINY

    ANY_NORMAL = ANY_FACE_NORMAL | BOX_FIRST | BOX_SECOND | BOX_THIRD
    ANY_SHINY = ANY_FACE_SHINY | BOX_FIRST_SHINY | BOX_SECOND_SHINY | BOX_THIRD_SHINY
