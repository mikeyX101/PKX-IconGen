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
