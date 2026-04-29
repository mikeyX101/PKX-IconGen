""" License 
    PKX-IconGen.Python - Python code for PKX-IconGen to interact with Blender
    Copyright (C) 2021-2026 Samuel Caron/mikeyX#4697

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

from .animation_name import AnimationName
from .camera import Camera
from .color import Color
from .object_shading import ObjectShading
from .texture import Texture


class RenderData(object):

    def __init__(self,
                 animation_name: Optional[AnimationName],
                 animation_frame: int,
                 main_camera: Optional[Camera], # While it is marked as Optional, it never will be since it is initiated once the model is loaded
                 secondary_camera: Optional[Camera],
                 removed_objects: List[str],
                 textures: Optional[List[Texture]], # Optional for compatibility, should always be not null
                 shading: Optional[ObjectShading], # Optional for compatibility, should always be not null
                 bg: Optional[Color],
                 glow: Optional[Color]):
        self.animation_name = animation_name or AnimationName.IDLE
        self.animation_frame = animation_frame

        self.main_camera = main_camera
        self.secondary_camera = secondary_camera

        self.removed_objects = removed_objects
        self.textures = textures or list[Texture]()
        self.shading = shading or ObjectShading.SMOOTH

        self.bg = bg or Color(0, 0, 0, 1)
        self.glow = glow or Color(1, 1, 1, 0)

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['RenderData']:
        if obj is None:
            return obj

        keys = obj.__dict__.keys()

        animation_name: Optional[AnimationName] = None
        if "animation_name" in keys:
            animation_name = AnimationName(obj.animation_name)

        main_camera: Optional[Camera] = None
        if "main_camera" in keys:
            main_camera = Camera.parse_obj(obj.main_camera)

        secondary_camera: Optional[Camera] = None
        if "secondary_camera" in keys:
            secondary_camera = Camera.parse_obj(obj.secondary_camera)

        textures: Optional[List[Texture]] = None
        if "textures" in keys:
            textures = list[Texture]()
            for texture in obj.textures:
                textures.append(Texture.parse_obj(texture))

        shading: Optional[ObjectShading] = None
        if "shading" in keys:
            shading = ObjectShading(obj.shading)

        bg: Optional[Color] = None
        if "bg" in keys:
            bg = Color.parse_obj(obj.bg)

        glow: Optional[Color] = None
        if "glow" in keys:
            glow = Color.parse_obj(obj.glow)

        return RenderData(
            animation_name,
            obj.animation_frame,
            main_camera,
            secondary_camera,
            obj.removed_objects,
            textures,
            shading,
            bg,
            glow
        )
