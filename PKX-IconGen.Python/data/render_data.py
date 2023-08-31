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
from typing import List

from .camera import Camera
from .color import Color
from .object_shading import ObjectShading
from .texture import Texture


class RenderData(object):

    def __init__(self,
                 animation_pose: int,
                 animation_frame: int,
                 main_camera: Camera,
                 secondary_camera: Optional[Camera],
                 removed_objects: List[str],
                 textures: Optional[List[Texture]],
                 shading: Optional[ObjectShading],
                 bg: Optional[Color],
                 glow: Optional[Color]):
        self.animation_pose = animation_pose
        self.animation_frame = animation_frame

        self.main_camera = main_camera
        self.secondary_camera = secondary_camera

        self.removed_objects = removed_objects
        self.textures = textures or list[Texture]()  # Optional for compatibility, should always be not null
        self.shading = shading or ObjectShading.FLAT  # Optional for compatibility, should always be not null

        self.bg = bg or Color(0, 0, 0, 1)
        self.glow = glow or Color(1, 1, 1, 0)

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['RenderData']:
        if obj is None:
            return obj

        secondary_camera: Optional[Camera] = None
        if "secondary_camera" in obj.__dict__.keys():
            secondary_camera = Camera.parse_obj(obj.secondary_camera)

        textures: Optional[List[Texture]] = None
        if "textures" in obj.__dict__.keys():
            textures = list[Texture]()
            for texture in obj.textures:
                textures.append(Texture.parse_obj(texture))

        shading: Optional[ObjectShading] = None
        if "shading" in obj.__dict__.keys():
            shading = ObjectShading(obj.shading)

        bg: Optional[Color] = None
        if "bg" in obj.__dict__.keys():
            bg = Color.parse_obj(obj.bg)

        glow: Optional[Color] = None
        if "glow" in obj.__dict__.keys():
            glow = Color.parse_obj(obj.glow)

        return RenderData(
            obj.animation_pose,
            obj.animation_frame,
            Camera.parse_obj(obj.main_camera),
            secondary_camera,
            obj.removed_objects,
            textures,
            shading,
            bg,
            glow
        )
