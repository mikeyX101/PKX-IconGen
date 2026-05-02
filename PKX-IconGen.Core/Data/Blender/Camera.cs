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
using JetBrains.Annotations;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// We need to let EF init properties

namespace PKXIconGen.Core.Data.Blender;

/// <summary>
/// Data for a Camera in a Blender scene.
/// </summary>
public readonly struct Camera : IJsonSerializableBlenderObject, IEquatable<Camera>
{
    [JsonPropertyName("pos")]
    public JsonSerializableVector3 Position { get; init; }
    [JsonPropertyName("focus")]
    public JsonSerializableVector3 FocusPoint { get; init; }

    [JsonPropertyName("fov")]
    public float FieldOfView { get; init; }
        
    [JsonPropertyName("is_ortho")]
    public bool IsOrthographic { get; init; }
    [JsonPropertyName("ortho_scale")]
    public float OrthographicScale { get; init; }

    [JsonPropertyName("light")]
    public Light Light { get; init; }
        
    [UsedImplicitly]
    public Camera(Vector3 position, Vector3 focusPoint, float fieldOfView, bool isOrthographic, float orthographicScale, Light light)
    {
        Position = new JsonSerializableVector3(position);
        FocusPoint = new JsonSerializableVector3(focusPoint);
        FieldOfView = fieldOfView;
        IsOrthographic = isOrthographic;
        OrthographicScale = orthographicScale;
        Light = light;
    }

    public bool Equals(Camera other)
    {
        return
            Position.Equals(other.Position) &&
            FocusPoint.Equals(other.FocusPoint) &&
            Math.Abs(FieldOfView - other.FieldOfView) < 0.000000001 &&
            IsOrthographic == other.IsOrthographic &&
            Math.Abs(OrthographicScale - other.OrthographicScale) < 0.000000001 &&
            Light.Equals(other.Light);
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

    public override int GetHashCode() => (Position, FocusPoint, FieldOfView, IsOrthographic, OrthographicScale, Light).GetHashCode();
            
}