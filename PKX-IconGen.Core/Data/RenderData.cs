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
using System.Text.Json.Serialization;
using PKXIconGen.Core.Data.Blender;
using PKXIconGen.Core.Data.Compatibility;

namespace PKXIconGen.Core.Data;

[JsonConverter(typeof(RenderDataJsonConverter))]
public class RenderData : IJsonSerializable, IEquatable<RenderData>, ICloneable
{
    [Obsolete("Used for old JSON compatibility. Use PRD.Model or PRD.Shiny.Model instead.")]
    private string? model;
    /// <summary>
    /// Model path. Can contain {{AssetsPath}} to represent the path to extracted assets.
    /// </summary>
    [JsonPropertyName("model"), Obsolete("Used for old JSON compatibility. Use PRD.Model or PRD.Shiny.Model instead.")]
    public string? Model
    {
        get => model;
        set => model = value;
    }

    [JsonPropertyName("animation_pose"), Obsolete("Used for old JSON compatibility. Use AnimationName instead.")]
    public ushort AnimationPose { get; init; }
    
    [JsonPropertyName("animation_name")]
    public AnimationName AnimationName { get; init; }
    
    [JsonPropertyName("animation_frame")]
    public ushort AnimationFrame { get; init; }

    // If null, Blender will figure out the starting camera
    [JsonPropertyName("main_camera")]
    public Camera? MainCamera { get; init; }
    [JsonPropertyName("secondary_camera")]
    public Camera? SecondaryCamera { get; init; }

    [JsonPropertyName("removed_objects")]
    public HashSet<string> RemovedObjects { get; init; }
        
    [JsonPropertyName("textures")]
    public List<Texture> Textures { get; init; }
        
    [JsonPropertyName("shading")]
    public ObjectShading ObjectShading { get; init; }

    /**
     * Background color for this render.
     * This is not used in Blender, instead we apply it during post-processing.
     * This also allows for transparent backgrounds or edge detected post-processing.
     */
    [JsonPropertyName("bg")]
    public Color Background { get; set; }

    /**
     * Glow color for this render.
     * This is not used in Blender, instead we apply it during post-processing.
     * If alpha is set to 0, the post-processing step for the glow is skipped.
     */
    [JsonPropertyName("glow")]
    public Color Glow { get; set; }
        
    public RenderData(RenderTarget target) {
        AnimationName = AnimationName.Idle;
        AnimationFrame = 0;

        MainCamera = null;
        SecondaryCamera = null;

        RemovedObjects = new HashSet<string>();
        Textures = new List<Texture>();

        ObjectShading = ObjectShading.Smooth;
            
        Background = target == RenderTarget.Face ? new Color(0,0,0,1) : new Color(0,0,0,0);
        Glow = target == RenderTarget.Face ? new Color(1, 1, 1, 0) : new Color(0, 0, 0, 1);
    }

    [JsonConstructor]
    public RenderData(
        AnimationName animationName, 
        ushort animationFrame, 
        Camera? mainCamera, 
        Camera? secondaryCamera, 
        HashSet<string> removedObjects, 
        List<Texture>? textures, 
        ObjectShading objectShading,
        Color background,
        Color glow)
    {
        AnimationName = animationName;
        AnimationFrame = animationFrame;

        MainCamera = mainCamera;
        SecondaryCamera = secondaryCamera;

        RemovedObjects = removedObjects;
        Textures = textures ?? [];

        ObjectShading = objectShading;

        Background = background;
        Glow = glow;
    }

    public bool Equals(RenderData? other)
    {
        return other is not null &&
               AnimationName == other.AnimationName &&
               AnimationFrame == other.AnimationFrame &&
               MainCamera.Equals(other.MainCamera) &&
               SecondaryCamera.Equals(other.SecondaryCamera) &&
               RemovedObjects.SequenceEqual(other.RemovedObjects) &&
               Textures.SequenceEqual(other.Textures) &&
               ObjectShading == other.ObjectShading &&
               Background.Equals(other.Background) &&
               Glow.Equals(other.Glow);
    }
    public override bool Equals(object? obj)
    {
        return obj is RenderData renderData && Equals(renderData);
    }

    public static bool operator ==(RenderData? left, RenderData? right)
    {
        return left?.Equals(right) ?? left is null && right is null;
    }

    public static bool operator !=(RenderData? left, RenderData? right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => (
        AnimationName, 
        AnimationFrame, 
        MainCamera, 
        SecondaryCamera, 
        RemovedObjects,
        Textures,
        ObjectShading,
        Background,
        Glow
    ).GetHashCode();

    public object Clone()
    {
        return new RenderData(
            AnimationName,
            AnimationFrame,
            MainCamera,
            SecondaryCamera,
            [..RemovedObjects],
            [..Textures],
            ObjectShading,
            Background,
            Glow
        );
    }
}