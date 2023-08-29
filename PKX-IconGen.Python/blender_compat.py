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


class MixInputs(object):

    def __init__(self, factor, color1, color2):
        self.factor = factor
        self.color1 = color1
        self.color2 = color2


class MappingInputs(object):

    def __init__(self, vector, location, rotation, scale):
        self.vector = vector
        self.location = location
        self.rotation = rotation
        self.scale = scale


class BumpInputs(object):

    def __init__(self, strength):
        self.strength = strength


class PrincipledBSDFInputs(object):

    def __init__(self,
                 base_color,
                 metallic,
                 specular,
                 roughness,
                 transmission_roughness,
                 emission_strength,
                 alpha,
                 normal):
        self.base_color = base_color
        self.metallic = metallic
        self.specular = specular
        self.roughness = roughness
        self.transmission_roughness = transmission_roughness
        self.emission_strength = emission_strength
        self.alpha = alpha
        self.normal = normal


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
    alpha=19,
    normal=20
)
principled_bsdf_out: PrincipledBSDFOutputs = PrincipledBSDFOutputs(
    bsdf=0
)
mapping_in: MappingInputs = MappingInputs(
    vector=0,
    location=1,
    rotation=2,
    scale=3
)
bump_in: BumpInputs = BumpInputs(
    strength=0
)
tex_image_in: TexImageInputs = TexImageInputs(
    vector=0
)
mix_in: MixInputs = MixInputs(
    factor=0,
    color1=1,
    color2=2
)

# Overrides
if (3, 0, 0) <= blender_ver:  # < (3, 6, 1):
    principled_bsdf_in = PrincipledBSDFInputs(
        base_color=0,
        metallic=6,
        specular=7,
        roughness=9,
        transmission_roughness=18,
        emission_strength=20,
        alpha=21,
        normal=22
    )
