using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data
{
    public class IconStyle
    {
        public static IconStyle[] GetIconStyles()
        {
            Game[] games = Enum.GetValues<Game>();
            return games.Select(game => new IconStyle(game)).ToArray();
        }

        public Game Game { get; set; }
        public string DisplayName => Game.GetName();

        private IconStyle(Game game)
        {
            Game = game;
        }
    }
}
