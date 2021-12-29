using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Color reported from Blender, storing each color value in the range [0..1].
    /// </summary>
    internal readonly struct Color
    {
        public float Red { get; init; }
        public float Green { get; init; }
        public float Blue { get; init; }

        public Color(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
