using System.Numerics;

namespace PKXIconGen.Core.Data.Blender
{
    internal readonly struct Light : IBlenderObject
    {
        public Vector3 Position { get; init; }
        public Vector3 RotationEulerXYZ { get; init; }

        public LightType Type { get; init; }
        public float Strength { get; init; }
        public Color Color { get; init; }

        public Light(Vector3 position, Vector3 rotationEulerXYZ, LightType type, float strength, Color color)
        {
            Position = position;
            RotationEulerXYZ = rotationEulerXYZ;
            Type = type;
            Strength = strength;
            Color = color;
        }
    }
}