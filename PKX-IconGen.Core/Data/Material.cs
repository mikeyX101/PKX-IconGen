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
using System.Text.Json.Serialization;
using PKXIconGen.Core.Data.Blender;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// We need to let EF init properties

namespace PKXIconGen.Core.Data;

public readonly struct Material: IJsonSerializable, IEquatable<Material>
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
        
    [JsonPropertyName("map")]
    public JsonSerializableVector2 Map { get; init; }
        
    [JsonConstructor]
    public Material(string name, JsonSerializableVector2 map)
    {
        Name = name;
        Map = map;
    }
    
    public bool Equals(Material other)
    {
        return
            Name == other.Name &&
            Map == other.Map;
    }
    public override bool Equals(object? obj)
    {
        return obj is Material material && Equals(material);
    }

    public static bool operator ==(Material left, Material right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Material left, Material right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => (Name, Map).GetHashCode();
}