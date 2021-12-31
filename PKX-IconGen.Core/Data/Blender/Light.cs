#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

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
using System.Numerics;
using System.Text.Json.Serialization;

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Data for one light in a Blender scene.
    /// </summary>
    internal readonly struct Light : IJsonSerializableBlenderObject, IEquatable<Light>
    {
        [JsonPropertyName("pos")]
        public JsonSerializableVector3 Position { get; init; }
        [JsonPropertyName("rot")]
        public JsonSerializableVector3 RotationEulerXYZ { get; init; }

        [JsonPropertyName("lightType")]
        public LightType Type { get; init; }
        [JsonPropertyName("strength")]
        public float Strength { get; init; }
        [JsonPropertyName("color")]
        public Color Color { get; init; }

        public Light(Vector3 position, Vector3 rotationEulerXYZ, LightType type, float strength, Color color)
        {
            Position = new(position);
            RotationEulerXYZ = new(rotationEulerXYZ);
            Type = type;
            Strength = strength;
            Color = color;
        }

        public static Light GetDefaultLight() => new();


        public bool Equals(Light other)
        {
            return 
                Position.Equals(other.Position) &&
                RotationEulerXYZ.Equals(other.RotationEulerXYZ) &&
                Type == other.Type &&
                Strength == other.Strength &&
                Color.Equals(other.Color);
        }
    }
}