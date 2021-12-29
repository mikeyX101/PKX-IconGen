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