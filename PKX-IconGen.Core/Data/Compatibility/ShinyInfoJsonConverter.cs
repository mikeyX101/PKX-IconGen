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

public sealed class ShinyInfoJsonConverter : JsonConverter<ShinyInfo>
{
    private static readonly IDictionary<string, string> JsonPropNames = Utils.GetJsonPropNames<ShinyInfo>();
    
    public override ShinyInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        ShinyColor? color1 = null;
        ShinyColor? color2 = null;
        string? model = null;
        RenderData? faceRender = null;
        BoxInfo? boxRender = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (faceRender is null)
                {
                    throw new JsonException("Face render data missing");
                }
                
                if (model is null)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    model = faceRender.Model;
#pragma warning restore CS0618 // Type or member is obsolete
                }

                // Discard color1 and color2 if we have a model, model has the priority
                if (model is not null)
                {
                    color1 = color2 = null;
                }
                
                return new ShinyInfo(color1, color2, model, faceRender, boxRender);
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
                    if (propertyName == JsonPropNames[nameof(ShinyInfo.Color1)])
                    {
                        color1 = JsonSerializer.Deserialize<ShinyColor>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(ShinyInfo.Color2)])
                    {
                        color2 = JsonSerializer.Deserialize<ShinyColor>(jsonData, options);
                    }
#pragma warning disable CS0618 // Type or member is obsolete
                    else if (propertyName == JsonPropNames[nameof(ShinyInfo.Render)] || propertyName == JsonPropNames[nameof(ShinyInfo.FaceRender)])
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        faceRender = JsonSerializer.Deserialize<RenderData>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(ShinyInfo.BoxRender)])
                    {
                        boxRender = JsonSerializer.Deserialize<BoxInfo>(jsonData, options);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.String)
            {
                if (propertyName == JsonPropNames[nameof(ShinyInfo.Model)])
                {
                    model = reader.GetString();
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, ShinyInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Color1 is not null)
        {
            writer.WritePropertyName(JsonPropNames[nameof(ShinyInfo.Color1)]);
            writer.WriteRawValue(JsonSerializer.Serialize(value.Color1, options), true);
        }

        if (value.Color2 is not null)
        {
            writer.WritePropertyName(JsonPropNames[nameof(ShinyInfo.Color2)]);
            writer.WriteRawValue(JsonSerializer.Serialize(value.Color2, options), true);
        }

        if (value.Model is not null)
        {
            writer.WriteString(JsonPropNames[nameof(ShinyInfo.Model)], value.Model);
        }
        
        writer.WritePropertyName(JsonPropNames[nameof(ShinyInfo.FaceRender)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.FaceRender, options), true);
        
        writer.WritePropertyName(JsonPropNames[nameof(ShinyInfo.BoxRender)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.BoxRender, options), true);
        
        writer.WriteEndObject();
    }
}