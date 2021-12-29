using System.Numerics;

namespace PKXIconGen.Core.Data.Blender
{
    internal readonly struct Camera : IBlenderObject
    {
        public Vector3 Position { get; init; }
        public Vector3 RotationEulerXYZ { get; init; }


        public int FieldOfView { get; init; }
    }
}