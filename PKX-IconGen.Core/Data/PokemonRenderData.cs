using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKXIconGen.Core.Data.Blender;

namespace PKXIconGen.Core.Data
{
    public class PokemonRenderData
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public bool BuiltIn { get; set; }
        internal Camera Camera { get; set; }
        internal Light[] Lights { get; set; }

        internal PokemonRenderData(string name, string model, bool builtIn, Camera camera, Light[] lights)
        {
            Name = name;
            Model = model;
            BuiltIn = builtIn;
            Camera = camera;
            Lights = lights;
        }
    }
}
