#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2022 mikeyX#4697

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
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data
{
    public enum Game : byte
    {
        Undefined = 0,
        PokemonColosseum = 1,
        PokemonXDGaleOfDarkness = 2,
        PokemonBattleRevolution = 3
    }

    public static class GameExtensions
    {
        public static string GetName(this Game game) => game switch
        {
            Game.PokemonColosseum => "Pokémon Colosseum",
            Game.PokemonXDGaleOfDarkness => "Pokémon XD: Gale of Darkness",
            Game.PokemonBattleRevolution => "Pokémon Battle Revolution",
            Game.Undefined or _ => "Undefined"
        };
    }
}
