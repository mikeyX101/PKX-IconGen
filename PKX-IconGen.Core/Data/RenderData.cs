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

namespace PKXIconGen.Core.Data
{
    public class RenderData : IJsonSerializable, IEquatable<RenderData>, ICloneable
    {
        /// <summary>
        /// Model path. Can contain {{AssetsPath}} to represent the path to extracted assets.
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("animation_pose")]
        public ushort AnimationPose { get; init; }
        [JsonPropertyName("animation_frame")]
        public ushort AnimationFrame { get; init; }

        [JsonPropertyName("main_camera")]
        public Camera MainCamera { get; init; }
        [JsonPropertyName("secondary_camera")]
        public Camera? SecondaryCamera { get; init; }

        [JsonPropertyName("removed_objects")]
        public SortedSet<string> RemovedObjects { get; init; }
        
        [JsonPropertyName("textures")]
        public List<Texture> Textures { get; init; }
        
        /**
         * Background color for this render.
         * This is not used in Blender, instead we apply it during post-processing.
         * This also allows for transparent backgrounds or edge detected post-processing.
         */
        [JsonPropertyName("bg")]
        public Color Background { get; set; } = new(0,0,0,1);

        /**
         * Glow color for this render.
         * This is not used in Blender, instead we apply it during post-processing.
         * If alpha is set to 0, the post-processing step for the glow is skipped.
         */
        [JsonPropertyName("glow")]
        public Color Glow { get; set; } = new(1, 1, 1, 0);

        public RenderData() {
            Model = null;

            AnimationPose = 0;
            AnimationFrame = 0;

            MainCamera = Camera.GetDefaultCamera();
            SecondaryCamera = null;

            RemovedObjects = new SortedSet<string>();
            Textures = new List<Texture>();
        }

        [JsonConstructor]
        public RenderData(string? model, ushort animationPose, ushort animationFrame, Camera mainCamera, Camera? secondaryCamera, SortedSet<string> removedObjects, List<Texture>? textures)
        {
            Model = model;

            AnimationPose = animationPose;
            AnimationFrame = animationFrame;

            MainCamera = mainCamera;
            SecondaryCamera = secondaryCamera;

            RemovedObjects = removedObjects;
            Textures = textures ?? new List<Texture>();
        }

        public bool Equals(RenderData? other)
        {
            return other is not null &&
                Model == other.Model &&
                AnimationPose == other.AnimationPose &&
                AnimationFrame == other.AnimationFrame &&
                MainCamera.Equals(other.MainCamera) &&
                (SecondaryCamera?.Equals(other.SecondaryCamera) ?? other.SecondaryCamera == null) &&
                RemovedObjects.SequenceEqual(other.RemovedObjects);
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

        public string? GetTrueModelPath(string? assetsPath)
        {
            return Utils.GetTrueModelPath(Model, assetsPath);
        }

        public override int GetHashCode() => (
            Model, 
            AnimationPose, 
            AnimationFrame, 
            MainCamera, 
            SecondaryCamera, 
            RemovedObjects,
            Textures,
            Background,
            Glow
        ).GetHashCode();

        public object Clone()
        {
            return new RenderData(
                Model, 
                AnimationPose, 
                AnimationFrame, 
                MainCamera, 
                SecondaryCamera,
                new SortedSet<string>(RemovedObjects),
                new List<Texture>(Textures)
            )
            {
                Background = this.Background,
                Glow = this.Glow
            };
        }
    }
}
