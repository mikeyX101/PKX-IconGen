#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2023 Samuel Caron/mikeyx

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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PKXIconGen.Core.Data.Compatibility;

public sealed class PokemonRenderDataJsonConverter : JsonConverter<PokemonRenderData>
{
    private static readonly IDictionary<string, string> JsonPropNames = Utils.GetJsonPropNames<PokemonRenderData>();
    
    public override PokemonRenderData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? name = null;
        string? outputName = null;
        string? model = null;
        RenderData? faceRender = null;
        BoxInfo? boxRender = null;
        ShinyInfo? shiny = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (name is null)
                {
                    throw new JsonException("Name missing");
                }
                if (faceRender is null)
                {
                    throw new JsonException("Face render data missing");
                }
                if (shiny is null)
                {
                    throw new JsonException("Shiny info missing");
                }
                
                if (model is null)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    model = faceRender.Model;
#pragma warning restore CS0618 // Type or member is obsolete
                    
                    if (model is null)
                    {
                        throw new JsonException("Model missing, also checked for deprecated model from face render data");
                    }
                }
                
                return new PokemonRenderData(name, outputName, model, faceRender, boxRender, shiny);
            }

            // Get the key.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();

            // Get the value.
            reader.Read();
            
            if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.StartObject)
            {
                string jsonData = JsonDocument.ParseValue(ref reader).RootElement.ToString();
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (propertyName == JsonPropNames[nameof(PokemonRenderData.Render)] || propertyName == JsonPropNames[nameof(PokemonRenderData.FaceRender)])
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        faceRender = JsonSerializer.Deserialize<RenderData>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(PokemonRenderData.BoxRender)])
                    {
                        boxRender = JsonSerializer.Deserialize<BoxInfo>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(PokemonRenderData.Shiny)])
                    {
                        shiny = JsonSerializer.Deserialize<ShinyInfo>(jsonData, options);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.String)
            {
                if (propertyName == JsonPropNames[nameof(PokemonRenderData.Name)])
                {
                    name = reader.GetString();
                }
                else if (propertyName == JsonPropNames[nameof(PokemonRenderData.OutputName)])
                {
                    outputName = reader.GetString();
                }
                else if (propertyName == JsonPropNames[nameof(PokemonRenderData.Model)])
                {
                    model = reader.GetString();
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, PokemonRenderData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString(JsonPropNames[nameof(PokemonRenderData.Name)], value.Name);
        if (!string.IsNullOrWhiteSpace(value.OutputName))
        {
            writer.WriteString(JsonPropNames[nameof(PokemonRenderData.OutputName)], value.OutputName);
        }
        writer.WriteString(JsonPropNames[nameof(PokemonRenderData.Model)], value.Model);
        
        writer.WritePropertyName(JsonPropNames[nameof(PokemonRenderData.FaceRender)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.FaceRender, options), true);
        
        writer.WritePropertyName(JsonPropNames[nameof(PokemonRenderData.BoxRender)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.BoxRender, options), true);
        
        writer.WritePropertyName(JsonPropNames[nameof(PokemonRenderData.Shiny)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.Shiny, options), true);
        
        writer.WriteEndObject();
    }
}