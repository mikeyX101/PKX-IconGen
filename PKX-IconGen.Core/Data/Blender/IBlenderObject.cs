using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Blender object that's in a 3D space.
    /// </summary>
    internal interface IBlenderObject
    {
        public Vector3 Position { get; }
        public Vector3 RotationEulerXYZ { get; }
    }
}
