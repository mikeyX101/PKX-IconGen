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
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using PKXIconGen.Core.Data.Blender;

namespace PKXIconGen.Core.Data;

public readonly struct Texture : IJsonSerializable, IEquatable<Texture>
{
    [JsonPropertyName("name")]
    public readonly string TextureName { get; init; }
    /**
     * Can contain {{AssetsPath}}
     */
    [JsonPropertyName("path")]
    public readonly string? ImagePath { get; init; }

    [JsonPropertyName("mats")]
    public readonly List<Material> Materials { get; init; }
    
    [JsonConstructor]
    public Texture(string textureName, string? imagePath, List<Material>? materials)
    {
        TextureName = textureName;
        ImagePath = imagePath;

        Materials = materials ?? new List<Material>();
    }
    
    public bool Equals(Texture other)
    {
        return
            TextureName == other.TextureName &&
            ImagePath == other.ImagePath &&
            Materials.SequenceEqual(other.Materials);
    }
    public override bool Equals(object? obj)
    {
        return obj is Texture texture && Equals(texture);
    }

    public static bool operator ==(Texture left, Texture right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Texture left, Texture right)
    {
        return !(left == right);
    }

    public readonly override int GetHashCode() => (TextureName, ImagePath, Materials).GetHashCode();
}