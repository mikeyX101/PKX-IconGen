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

import bpy


class MappingInputs(object):

    def __init__(self, location):
        self.location = location


class PrincipledBSDFInputs(object):

    def __init__(self,
                 base_color,
                 metallic,
                 specular,
                 roughness,
                 transmission_roughness,
                 emission_strength,
                 alpha):
        self.base_color = base_color
        self.metallic = metallic
        self.specular = specular
        self.roughness = roughness
        self.transmission_roughness = transmission_roughness
        self.emission_strength = emission_strength
        self.alpha = alpha


class PrincipledBSDFOutputs(object):

    def __init__(self, bsdf):
        self.bsdf = bsdf


class TexImageInputs(object):

    def __init__(self, vector):
        self.vector = vector


blender_ver = bpy.app.version

# Defaults, Blender 2.93 LTS
principled_bsdf_in: PrincipledBSDFInputs = PrincipledBSDFInputs(
    base_color=0,
    metallic=4,
    specular=5,
    roughness=7,
    transmission_roughness=16,
    emission_strength=18,
    alpha=19
)
principled_bsdf_out: PrincipledBSDFOutputs = PrincipledBSDFOutputs(
    bsdf=0
)
mapping_in: MappingInputs = MappingInputs(
    location=1
)
tex_image_in: TexImageInputs = TexImageInputs(
    vector=0
)

# Overrides
if (3, 0, 0) <= blender_ver < (3, 3, 0):
    principled_bsdf_in = PrincipledBSDFInputs(
        base_color=0,
        metallic=6,
        specular=7,
        roughness=9,
        transmission_roughness=18,
        emission_strength=20,
        alpha=21
    )
