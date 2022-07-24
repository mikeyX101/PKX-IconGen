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


class PrincipaledBSDFInputs(object):

    def __init__(self, metallic, specular, roughness):
        self.metallic = metallic
        self.specular = specular
        self.roughness = roughness


class PrincipaledBSDFOutputs(object):

    def __init__(self, bsdf):
        self.bsdf = bsdf


class TexImageInputs(object):

    def __init__(self, vector):
        self.vector = vector


blender_ver = bpy.app.version

# Defaults, Blender 2.93 LTS
principaled_bsdf_in: PrincipaledBSDFInputs = PrincipaledBSDFInputs(
    metallic=4,
    specular=5,
    roughness=7
)
principaled_bsdf_out: PrincipaledBSDFOutputs = PrincipaledBSDFOutputs(
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
    principaled_bsdf_in = PrincipaledBSDFInputs(
        metallic=6,
        specular=7,
        roughness=9
    )
