from data.color import Color
from data.light import LightType
from data.light import Light
from data.vector import Vector
from data.camera import Camera
from data.shiny_info import ShinyInfo
from data.pokemon_render_data import PokemonRenderData

import typing

data: PokemonRenderData = PokemonRenderData(
    "Absol",
    "absol.pkx.dat", 
    0, 
    0, 
    ShinyInfo(0, 0, alt_model="rare_absol.pkx.dat"), 
    Camera(Vector(0,0,0), Vector(0,0,0), 0), 
    None, 
    [Light(Vector(0,0,0), Vector(0,0,0), LightType.AREA, 0, Color(0,0,0))], 
    [])

json_data: str = data.to_json()
print(json_data)

new_data: PokemonRenderData = PokemonRenderData.from_json(json_data)
print(new_data.to_json())