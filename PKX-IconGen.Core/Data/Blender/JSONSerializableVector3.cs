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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Wrapper for Vector3 so that it can be serialized with the JSONSerializer.
    /// The original Vector3 struct can be accessed with the Vector property.
    /// </summary>
    internal struct JSONSerializableVector3
    {
        [JsonIgnore]
        public Vector3 Vector { get; set; }

        [JsonPropertyName("x")]
        public float X => Vector.X;

        [JsonPropertyName("y")]
        public float Y => Vector.Y;

        [JsonPropertyName("z")]
        public float Z => Vector.Z;

        public JSONSerializableVector3(Vector3 vector)
        {
            Vector = vector;
        }

        [JsonConstructor]
        public JSONSerializableVector3(float x, float y, float z)
        {
            Vector = new(x, y, z);
        }
    }
}
