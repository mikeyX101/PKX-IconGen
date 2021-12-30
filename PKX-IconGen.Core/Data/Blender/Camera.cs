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

using System.Numerics;
using System.Text.Json.Serialization;

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Data for a Camera in a Blender scene.
    /// </summary>
    internal readonly struct Camera : ISerializableBlenderObject
    {
        [JsonPropertyName("pos")]
        public JSONSerializableVector3 Position { get; init; }
        [JsonPropertyName("rot")]
        public JSONSerializableVector3 RotationEulerXYZ { get; init; }

        [JsonPropertyName("fov")]
        public int FieldOfView { get; init; }

        public Camera(Vector3 position, Vector3 rotationEulerXYZ, int fieldOfView)
        {
            Position = new(position);
            RotationEulerXYZ = new(rotationEulerXYZ);
            FieldOfView = fieldOfView;
        }

        public static Camera GetDefaultCamera() => new();
    }
}