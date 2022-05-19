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


class RenderData(object):
    
    def __init__(self, 
                 model: str, 
                 animation_pose: int, 
                 animation_frame: int, 
                 main_camera: Camera, 
                 secondary_camera: Optional[Camera],
                 removed_objects: List[str]):
        self.model = model

        self.animation_pose = animation_pose
        self.animation_frame = animation_frame

        self.main_camera = main_camera
        self.secondary_camera = secondary_camera

        self.removed_objects = removed_objects

    @staticmethod
    def parse_obj(obj: Optional[any]) -> Optional['RenderData']:
        if obj is None:
            return obj

        secondary_camera: Optional[Camera] = None
        try:
            secondary_camera = Camera.parse_obj(obj.secondary_camera)
        except AttributeError:
            pass

        return RenderData(
            obj.model,
            obj.animation_pose,
            obj.animation_frame,
            Camera.parse_obj(obj.main_camera),
            secondary_camera,
            obj.removed_objects)
