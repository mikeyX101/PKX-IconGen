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

namespace PKXIconGen.Core.Data.Blender;

/// <summary>
/// Wrapper for <see cref="Vector3"/> so that it can be serialized with the JSONSerializer.
/// The original <see cref="Vector3"/> struct can be accessed with the <see cref="Vector"/> property.
/// </summary>
public readonly struct JsonSerializableVector3 : IJsonSerializable, IEquatable<JsonSerializableVector3>
{
    [JsonIgnore]
    public Vector3 Vector { get; }

    [JsonPropertyName("x")]
    public float X => Vector.X;

    [JsonPropertyName("y")]
    public float Y => Vector.Y;

    [JsonPropertyName("z")]
    public float Z => Vector.Z;

    public JsonSerializableVector3(Vector3 vector)
    {
        Vector = vector;
    }

    [JsonConstructor]
    public JsonSerializableVector3(float x, float y, float z)
    {
        Vector = new Vector3((float)Math.Round(x, 3), (float)Math.Round(y, 3), (float)Math.Round(z, 3));
    }

    public bool Equals(JsonSerializableVector3 other)
    {
        return Vector.Equals(other.Vector);
    }
    public override bool Equals(object? obj)
    {
        return obj is JsonSerializableVector3 vector && Equals(vector);
    }

    public static bool operator ==(JsonSerializableVector3 left, JsonSerializableVector3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JsonSerializableVector3 left, JsonSerializableVector3 right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => Vector.GetHashCode();
}
    
/// <summary>
/// Wrapper for <see cref="Vector2"/> so that it can be serialized with the JSONSerializer.
/// The original <see cref="Vector2"/> struct can be accessed with the <see cref="Vector"/> property.
/// </summary>
public readonly struct JsonSerializableVector2 : IJsonSerializable, IEquatable<JsonSerializableVector2>
{
    [JsonIgnore]
    public Vector2 Vector { get; }

    [JsonPropertyName("x")]
    public float X => Vector.X;

    [JsonPropertyName("y")]
    public float Y => Vector.Y;

    public JsonSerializableVector2(Vector2 vector)
    {
        Vector = vector;
    }

    [JsonConstructor]
    public JsonSerializableVector2(float x, float y)
    {
        Vector = new Vector2((float)Math.Round(x, 3), (float)Math.Round(y, 3));
    }

    public bool Equals(JsonSerializableVector2 other)
    {
        return Vector.Equals(other.Vector);
    }
    public override bool Equals(object? obj)
    {
        return obj is JsonSerializableVector2 vector && Equals(vector);
    }

    public static bool operator ==(JsonSerializableVector2 left, JsonSerializableVector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JsonSerializableVector2 left, JsonSerializableVector2 right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => Vector.GetHashCode();
}