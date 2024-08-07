﻿#region License
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
using System.Text.Json.Serialization;
using JetBrains.Annotations;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// We need to let EF init properties

namespace PKXIconGen.Core.Data.Blender;

/// <summary>
/// Data for a light in a Blender scene.
/// </summary>
public readonly struct Light : IJsonSerializable, IEquatable<Light>
{
    [JsonPropertyName("type")]
    public LightType Type { get; init; }
    [JsonPropertyName("strength")]
    public float Strength { get; init; }
    [JsonPropertyName("color")]
    public Color Color { get; init; }
    [JsonPropertyName("distance")]
    public float Distance { get; init; }
        
    [UsedImplicitly]
    public Light(LightType type, float strength, Color color, float distance)
    {
        Type = type;
        Strength = strength;
        Color = color;
        Distance = distance;
    }

    public bool Equals(Light other)
    {
        return 
            Type == other.Type &&
            Math.Abs(Strength - other.Strength) < 0.000000001 &&
            Color.Equals(other.Color) &&
            Math.Abs(Distance - other.Distance) < 0.000000001;
    }
    public override bool Equals(object? obj)
    {
        return obj is Light light && Equals(light);
    }

    public static bool operator ==(Light left, Light right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Light left, Light right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => (Type, Strength, Color, Distance).GetHashCode();

    public static Light GetDefaultLight(RenderTarget target) => target switch
    {
        RenderTarget.Face => new Light(LightType.Area, 250f, Color.GetDefaultColor(), 5f),
        RenderTarget.Box => new Light(LightType.Area, 650f, Color.GetDefaultColor(), 13f),
        RenderTarget.All or _ => throw new ArgumentException("Unknown RenderTarget.", nameof(target))
    };
}