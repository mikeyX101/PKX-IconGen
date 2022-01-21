""" License 
    PKX-IconGen.Python.Core - Core Python classes for PKX-IconGen to interact with Blender
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

import copy
import json
import typing
from typing import List

from data.camera import Camera
from data.color import Color
from data.light import Light, LightType
from data.shiny_info import ShinyInfo
from data.vector import Vector

class PokemonRenderData(object):

    def __init__(self,
                 name: str,
                 model: str, 
                 animation_pose: int, 
                 animation_frame: int, 
                 shiny: ShinyInfo, 
                 main_camera: Camera, 
                 secondary_camera: typing.Optional[Camera], 
                 main_lights: List[Light], 
                 secondary_lights: List[Light]):

        self.name = name
        self.model = model
        self.animation_pose = animation_pose
        self.animation_frame = animation_frame
        self.shiny = shiny
        self.main_camera = main_camera
        self.secondary_camera = secondary_camera
        self.main_lights = main_lights
        self.secondary_lights = secondary_lights

    def to_json(self) -> str:
        self_dict = vars(copy.deepcopy(self))
        self_dict["shiny"] = vars(self.shiny)
        if self.shiny.filter is not None:
            self_dict["shiny"]["filter"] = vars(self.shiny.color)
        else:
            del self_dict["shiny"]["filter"]

        self_dict["main_camera"] = vars(self.main_camera)
        self_dict["main_camera"]["pos"] = vars(self.main_camera.pos)
        self_dict["main_camera"]["rot"] = vars(self.main_camera.rot)
        if self.secondary_camera is not None:
            self_dict["secondary_camera"] = vars(self.secondary_camera)
            self_dict["secondary_camera"]["pos"] = vars(self.secondary_camera.pos)
            self_dict["secondary_camera"]["rot"] = vars(self.secondary_camera.rot)
        else:
            del self_dict["secondary_camera"]

        main_lights : List[Light] = []
        for light in self.main_lights:
            light_dict = vars(light)
            light_dict["pos"] = vars(light.pos)
            light_dict["rot"] = vars(light.rot)
            light_dict["light_type"] = light.light_type.value
            light_dict["color"] = vars(light.color)
            main_lights.append(light_dict)
        self_dict["main_lights"] = main_lights
            
        secondary_lights : List[Light] = []
        for light in self.secondary_lights:
            light_dict = vars(light)
            light_dict["pos"] = vars(light.pos)
            light_dict["rot"] = vars(light.rot)
            light_dict["light_type"] = light.light_type.value
            light_dict["color"] = vars(light.color)
            secondary_lights.append(light_dict)
        self_dict["secondary_lights"] = secondary_lights

        return json.dumps(self_dict)

    @staticmethod
    def from_json(json_str: str) -> 'PokemonRenderData':
        json_dict = json.loads(json_str)

        shiny: ShinyInfo
        if "filter" in json_dict["shiny"]:
            color_filter = Color(
                json_dict["shiny"]["filter"]["r"], 
                json_dict["shiny"]["filter"]["g"], 
                json_dict["shiny"]["filter"]["b"])

            shiny = ShinyInfo(
                json_dict["shiny"]["animation_pose"], 
                json_dict["shiny"]["animation_frame"], 
                color=color_filter)
        else:
            shiny = ShinyInfo(
                json_dict["shiny"]["animation_pose"], 
                json_dict["shiny"]["animation_frame"], 
                alt_model=json_dict["shiny"]["alt_model"])

        main_camera: Camera = Camera(
            Vector(json_dict["main_camera"]["pos"]["x"], json_dict["main_camera"]["pos"]["y"], json_dict["main_camera"]["pos"]["z"]),
            Vector(json_dict["main_camera"]["rot"]["x"], json_dict["main_camera"]["rot"]["y"], json_dict["main_camera"]["rot"]["z"]),
            json_dict["main_camera"]["fov"])

        secondary_camera: typing.Optional[Camera] = None
        if "secondary_camera" in json_dict:
            secondary_camera: Camera = Camera(
                Vector(json_dict["secondary_camera"]["pos"]["x"], json_dict["secondary_camera"]["pos"]["y"], json_dict["secondary_camera"]["pos"]["z"]),
                Vector(json_dict["secondary_camera"]["rot"]["x"], json_dict["secondary_camera"]["rot"]["y"], json_dict["secondary_camera"]["rot"]["z"]),
                json_dict["secondary_camera"]["fov"])

        main_lights: List[Light] = []
        for light in json_dict["main_lights"]:
            main_lights.append(
                Light(
                    Vector(light["pos"]["x"], light["pos"]["y"], light["pos"]["z"]),
                    Vector(light["rot"]["x"], light["rot"]["y"], light["rot"]["z"]),
                    LightType(light["light_type"]),
                    light["strength"],
                    Color(light["color"]["r"], light["color"]["g"], light["color"]["b"])
                )
            );

        secondary_lights: List[Light] = []
        for light in json_dict["secondary_lights"]:
            secondary_lights.append(
                Light(
                    Vector(light["pos"]["x"], light["pos"]["y"], light["pos"]["z"]),
                    Vector(light["rot"]["x"], light["rot"]["y"], light["rot"]["z"]),
                    LightType(light["light_type"]),
                    light["strength"],
                    Color(light["color"]["r"], light["color"]["g"], light["color"]["b"])
                )
            );

        return PokemonRenderData(
            json_dict["name"],
            json_dict["model"],
            json_dict["animation_pose"],
            json_dict["animation_frame"],
            shiny,
            main_camera,
            secondary_camera,
            main_lights,
            secondary_lights)