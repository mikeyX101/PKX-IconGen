#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2026 Samuel Caron/mikeyx

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
using PKXIconGen.Core.Data.Blender;

namespace PKXIconGen.Core.Data.Compatibility;

public sealed class RenderDataJsonConverter : JsonConverter<RenderData>
{
    private static readonly IDictionary<string, string> JsonPropNames = Utils.GetJsonPropNames<RenderData>();
    
    public override RenderData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? model = null;
        ushort? animationPose = null;
        AnimationName? animationName = null;
        ushort? animationFrame = null;
        Camera? mainCamera = null;
        Camera? secondaryCamera = null;
        List<string>? removedObjects = null;
        List<Texture>? textures = null;
        ObjectShading? objectShading = null;
        Color? background = null;
        Color? glow = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (animationName is null)
                {
                    if (animationPose is not null)
                    {
                        if (animationPose.Value <= 4)
                        {
                            animationName = (AnimationName)animationPose.Value;
                        }
                        else
                        {
                            animationName = AnimationName.Idle;
                            PKXCore.Logger.Warning("Animation pose value ({AnimationPose}) out of bounds, setting animation to Idle", animationPose);
                        }
                    }
                    else
                    {
                        throw new JsonException("Animation name and pose missing");
                    }
                }
                
                if (animationFrame is null)
                {
                    throw new JsonException("Animation frame missing");
                }
                if (removedObjects is null)
                {
                    throw new JsonException("Removed objects missing");
                }
                if (textures is null)
                {
                    throw new JsonException("Textures missing");
                }
                if (objectShading is null)
                {
                    throw new JsonException("Object shading missing");
                }
                if (background is null)
                {
                    throw new JsonException("Background color missing");
                }
                if (glow is null)
                {
                    throw new JsonException("Glow color missing");
                }
                
                return new RenderData(
                    animationName.Value, 
                    animationFrame.Value, 
                    mainCamera,
                    secondaryCamera,
                    new HashSet<string>(removedObjects),
                    textures,
                    objectShading.Value,
                    background.Value,
                    glow.Value
                ) {
#pragma warning disable CS0618 // Type or member is obsolete
                    Model = model // still need to read model if old json, so it can be put in PRD/ShinyInfo
#pragma warning restore CS0618 // Type or member is obsolete
                };
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
                    if (propertyName == JsonPropNames[nameof(RenderData.MainCamera)])
                    {
                        mainCamera = JsonSerializer.Deserialize<Camera>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(RenderData.SecondaryCamera)])
                    {
                        secondaryCamera = JsonSerializer.Deserialize<Camera>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(RenderData.Background)])
                    {
                        background = JsonSerializer.Deserialize<Color>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(RenderData.Glow)])
                    {
                        glow = JsonSerializer.Deserialize<Color>(jsonData, options);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.StartArray)
            {
                string jsonData = JsonDocument.ParseValue(ref reader).RootElement.ToString();
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    if (propertyName == JsonPropNames[nameof(RenderData.RemovedObjects)])
                    {
                        removedObjects = JsonSerializer.Deserialize<List<string>>(jsonData, options);
                    }
                    else if (propertyName == JsonPropNames[nameof(RenderData.Textures)])
                    {
                        textures = JsonSerializer.Deserialize<List<Texture>>(jsonData, options);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.Number)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (propertyName == JsonPropNames[nameof(RenderData.AnimationPose)])
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    animationPose = reader.GetUInt16();
                }
                else if (propertyName == JsonPropNames[nameof(RenderData.AnimationName)])
                {
                    int value = reader.GetInt32();
                    if (value is > 4 or < 0)
                    {
                        throw new JsonException("Invalid animation name value, must be between 0 and 4.");
                    }

                    animationName = (AnimationName)value;
                }
                else if (propertyName == JsonPropNames[nameof(RenderData.AnimationFrame)])
                {
                    animationFrame = reader.GetUInt16();
                }
                else if (propertyName == JsonPropNames[nameof(RenderData.ObjectShading)])
                {
                    int value = reader.GetInt32();
                    if (value is > 1 or < 0)
                    {
                        throw new JsonException("Invalid object shading value, must be between 0 and 1.");
                    }

                    objectShading = (ObjectShading)value;
                }
            }
            else if (!string.IsNullOrWhiteSpace(propertyName) && reader.TokenType == JsonTokenType.String)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (propertyName == JsonPropNames[nameof(RenderData.Model)])
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    model = reader.GetString();
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, RenderData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteNumber(JsonPropNames[nameof(RenderData.AnimationName)], (byte)value.AnimationName);
        writer.WriteNumber(JsonPropNames[nameof(RenderData.AnimationFrame)], value.AnimationFrame);
        
        writer.WritePropertyName(JsonPropNames[nameof(RenderData.MainCamera)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.MainCamera, options), true);

        if (value.SecondaryCamera is not null)
        {
            writer.WritePropertyName(JsonPropNames[nameof(RenderData.SecondaryCamera)]);
            writer.WriteRawValue(JsonSerializer.Serialize(value.SecondaryCamera, options), true);
        }
        
        writer.WriteStartArray(JsonPropNames[nameof(RenderData.RemovedObjects)]);
        foreach (string item in value.RemovedObjects)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
        
        writer.WriteStartArray(JsonPropNames[nameof(RenderData.Textures)]);
        foreach (Texture item in value.Textures)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(item, options), true);
        }
        writer.WriteEndArray();

        writer.WriteNumber(JsonPropNames[nameof(RenderData.ObjectShading)], (byte)value.ObjectShading);
        
        writer.WritePropertyName(JsonPropNames[nameof(RenderData.Background)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.Background, options), true);
        
        writer.WritePropertyName(JsonPropNames[nameof(RenderData.Glow)]);
        writer.WriteRawValue(JsonSerializer.Serialize(value.Glow, options), true);
        
        writer.WriteEndObject();
    }
}