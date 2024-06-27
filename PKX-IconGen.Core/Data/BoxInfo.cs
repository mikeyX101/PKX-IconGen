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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace PKXIconGen.Core.Data;

public enum BoxAnimationFrame : byte
{
    First = 0,
    Second = 1,
    Third = 2
}
    
public static class BoxAnimationFrameExtensions
{
    public static BoxAnimation GetBoxAnimation(this BoxAnimationFrame frame)
    {
        BoxAnimation[] boxAnimations = BoxAnimation.GetBoxAnimations();
        return boxAnimations.First(a => a.Frame == frame);
    }
        
    public static string GetName(this BoxAnimationFrame frame) => frame switch
    {
        BoxAnimationFrame.First => "First",
        BoxAnimationFrame.Second => "Second",
        BoxAnimationFrame.Third => "Third",
        _ => ""
    };
}
    
public class BoxAnimation
{
    public static BoxAnimation[] GetBoxAnimations()
    {
        BoxAnimationFrame[] frames = Enum.GetValues<BoxAnimationFrame>();
        return frames.Select(frame => new BoxAnimation(frame)).ToArray();
    }

    public BoxAnimationFrame Frame { get; }
    public string DisplayName => Frame.GetName();

    private BoxAnimation(BoxAnimationFrame frame)
    {
        Frame = frame;
    }
}
    
public class BoxInfo : IJsonSerializable, IEquatable<BoxInfo>, ICloneable
{
        
    [JsonPropertyName("first")]
    public RenderData First { get; init; }
        
    [JsonPropertyName("second")]
    public RenderData Second { get; init; }
        
    [JsonPropertyName("third")]
    public RenderData Third { get; init; }

    public BoxInfo()
    {
        First = new RenderData(RenderTarget.Box);
        Second = new RenderData(RenderTarget.Box);
        Third = new RenderData(RenderTarget.Box);
    }

    [JsonConstructor]
    public BoxInfo(
        RenderData first,
        RenderData second,
        RenderData third)
    {
        First = first;
        Second = second;
        Third = third;
    }

    public RenderData GetBoxRenderData(BoxAnimationFrame frame) => frame switch
    {
        BoxAnimationFrame.First => First,
        BoxAnimationFrame.Second => Second,
        BoxAnimationFrame.Third => Third,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Somehow got an unknown BoxAnimationFrame")
    };
        
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "False on init")]
    public void ResetTexturesAndRemovedObjects()
    {
        if (First?.Textures is not null && First.Textures.Count != 0)
        {
            First.Textures.Clear();
            CoreManager.Logger.Information("First Box Model changed while having textures set up, removing to avoid conflicts");
        }
        if (Second?.Textures is not null && Second.Textures.Count != 0)
        {
            Second.Textures.Clear();
            CoreManager.Logger.Information("Second Box Model changed while having textures set up, removing to avoid conflicts");
        }
        if (Third?.Textures is not null && Third.Textures.Count != 0)
        {
            Third.Textures.Clear();
            CoreManager.Logger.Information("Third Box Model changed while having textures set up, removing to avoid conflicts");
        }
            
        if (First?.RemovedObjects is not null && First.RemovedObjects.Count != 0)
        {
            First.RemovedObjects.Clear();
            CoreManager.Logger.Information("First Box Model changed while having removed objects, resetting to avoid conflicts");
        }
        if (Second?.RemovedObjects is not null && Second.RemovedObjects.Count != 0)
        {
            Second.RemovedObjects.Clear();
            CoreManager.Logger.Information("Second Box Model changed while having removed objects, resetting to avoid conflicts");
        }
        if (Third?.RemovedObjects is not null && Third.RemovedObjects.Count != 0)
        {
            Third.RemovedObjects.Clear();
            CoreManager.Logger.Information("Third Box Model changed while having removed objects, resetting to avoid conflicts");
        }
    }
        
    public bool Equals(BoxInfo? other)
    {
        return other is not null &&
               First == other.First &&
               Second == other.Second &&
               Third == other.Third;
    }
    public override bool Equals(object? obj)
    {
        return obj is BoxInfo boxInfo && Equals(boxInfo);
    }

    public static bool operator ==(BoxInfo? left, BoxInfo? right)
    {
        return left?.Equals(right) ?? left is null && right is null;
    }

    public static bool operator !=(BoxInfo? left, BoxInfo? right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => (
        First, 
        Second, 
        Third
    ).GetHashCode();

    public object Clone()
    {
        return new BoxInfo(
            (RenderData)First.Clone(),
            (RenderData)Second.Clone(),
            (RenderData)Third.Clone()
        );
    }
}