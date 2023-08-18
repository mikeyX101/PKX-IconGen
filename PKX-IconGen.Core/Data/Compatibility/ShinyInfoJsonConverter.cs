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
        
        IDictionary<string, string> jsonPropNames = Utils.GetJsonPropNames<ShinyInfo>();

        ShinyColor? color1 = null;
        ShinyColor? color2 = null;
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
                
                return new ShinyInfo(color1, color2, faceRender, boxRender);
            }

            // Get the key.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();

            // Get the value.
            reader.Read();
            string jsonData = JsonDocument.ParseValue(ref reader).RootElement.ToString();
            if (!string.IsNullOrWhiteSpace(propertyName) && !string.IsNullOrWhiteSpace(jsonData))
            {
                if (propertyName == jsonPropNames["Color1"])
                {
                    color1 = JsonSerializer.Deserialize<ShinyColor>(jsonData, options);
                }
                else if (propertyName == jsonPropNames["Color2"])
                {
                    color2 = JsonSerializer.Deserialize<ShinyColor>(jsonData, options);
                }
                else if (propertyName == jsonPropNames["Render"] || propertyName == jsonPropNames["FaceRender"])
                {
                    faceRender = JsonSerializer.Deserialize<RenderData>(jsonData, options);
                }
                else if (propertyName == jsonPropNames["BoxRender"])
                {
                    boxRender = JsonSerializer.Deserialize<BoxInfo>(jsonData, options);
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, ShinyInfo value, JsonSerializerOptions options)
    {
        IDictionary<string, string> jsonPropNames = Utils.GetJsonPropNames<ShinyInfo>();
        
        writer.WriteStartObject();
        
        writer.WritePropertyName(jsonPropNames["Color1"]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.Color1, options), true);
        
        writer.WritePropertyName(jsonPropNames["Color2"]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.Color2, options), true);
        
        writer.WritePropertyName(jsonPropNames["FaceRender"]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.FaceRender, options), true);
        
        writer.WritePropertyName(jsonPropNames["BoxRender"]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.BoxRender, options), true);
        
        writer.WriteEndObject();
    }
}