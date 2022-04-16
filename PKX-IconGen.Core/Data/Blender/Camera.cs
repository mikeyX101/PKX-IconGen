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
    /// Data for a Camera in a Blender scene.
    /// </summary>
    public readonly struct Camera : IJsonSerializableBlenderObject, IEquatable<Camera>
    {
        [JsonPropertyName("pos")]
        public JsonSerializableVector3 Position { get; init; }
        [JsonPropertyName("rot")]
        public JsonSerializableVector3 RotationEuler { get; init; }

        [JsonPropertyName("fov")]
        public float FieldOfView { get; init; }

        public Camera(Vector3 position, Vector3 rotationEulerXYZ, float fieldOfView)
        {
            Position = new(position);
            RotationEuler = new(rotationEulerXYZ);
            FieldOfView = fieldOfView;
        }

        public bool Equals(Camera other)
        {
            return
                Position.Equals(other.Position) &&
                RotationEuler.Equals(other.RotationEuler) &&
                FieldOfView == other.FieldOfView;
        }
        public override bool Equals(object? obj)
        {
            return obj is Camera camera && Equals(camera);
        }

        public static bool operator ==(Camera left, Camera right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Camera left, Camera right)
        {
            return !(left == right);
        }

        public readonly override int GetHashCode() => (Position, RotationEuler, FieldOfView).GetHashCode();

        public static Camera GetDefaultCamera() => new(new(14f, -13.5f, 5.5f), new(86.8f, 0f, 54f), 40f);
    }
}