#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2024 Samuel Caron/mikeyX

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

namespace PKXIconGen.Core.Data;

public enum TextureTargetChoice : byte
{
    Original, Dolphin
}

public enum OutputChoice : byte
{
    Face, Box, BoxShiny
}

public readonly struct TextureNames(Names dolphinNames, Names originalNames)
{
    [JsonPropertyName("dol"), UsedImplicitly]
    public Names DolphinNames { get; init; } = dolphinNames;
    [JsonPropertyName("og"), UsedImplicitly]
    public Names OriginalNames { get; init; } = originalNames;
    
    public string? GetName(TextureTargetChoice textureChoice, OutputChoice outputChoice) => textureChoice switch
    {
        TextureTargetChoice.Dolphin => DolphinNames.GetName(outputChoice),
        TextureTargetChoice.Original => OriginalNames.GetName(outputChoice),
        _ => throw new ArgumentOutOfRangeException(nameof(textureChoice), textureChoice, "Somehow got an unknown TextureTargetChoice")
    };
}

public readonly struct Names
{
    [JsonPropertyName("f"), UsedImplicitly]
    public string? Face { get; init; }

    [JsonPropertyName("b"), UsedImplicitly]
    public string? Box { get; init; }

    [JsonPropertyName("bs"), UsedImplicitly]
    public string? BoxShiny { get; init; }
    
    public string? GetName(OutputChoice outputChoice) => outputChoice switch
    {
        OutputChoice.Face => Face,
        OutputChoice.Box => Box, 
        OutputChoice.BoxShiny => BoxShiny,
        _ => throw new ArgumentOutOfRangeException(nameof(outputChoice), outputChoice, "Somehow got an unknown OutputChoice")
    };
}