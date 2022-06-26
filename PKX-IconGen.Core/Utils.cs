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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PKXIconGen.Core
{
    public static class Utils
    {
        public static float ConvertRange(
            int originalStart, int originalEnd, // original range
            float newStart, float newEnd, // desired range
            int value) // value to convert
        {
            if (originalStart > value) {
                throw new ArgumentException("Value was smaller than the original range.", nameof(value));
            }
            else if (originalEnd < value)
            {
                throw new ArgumentException("Value was greater than the original range.", nameof(value));
            }

            float scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + ((value - originalStart) * scale);
        }

        public static float ConvertRange(
            uint originalStart, uint originalEnd, // original range
            float newStart, float newEnd, // desired range
            uint value) // value to convert
        {
            if (originalStart > value) {
                throw new ArgumentException("Value was smaller than the original range.", nameof(value));
            }
            else if (originalEnd < value)
            {
                throw new ArgumentException("Value was greater than the original range.", nameof(value));
            }

            float scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + (value - originalStart) * scale;
        }

        public static float ConvertRange(
            float originalStart, float originalEnd, // original range
            float newStart, float newEnd, // desired range
            float value) // value to convert
        {
            if (originalStart > value) {
                throw new ArgumentException("Value was smaller than the original range.", nameof(value));
            }
            else if (originalEnd < value)
            {
                throw new ArgumentException("Value was greater than the original range.", nameof(value));
            }

            float scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + (value - originalStart) * scale;
        }

        // Saturation and Value to 1
        public static uint HueToRgb(float hue)
        {
            hue = Math.Clamp(hue, 0, 360);
            const float sat = 1;
            const float val = 1;

            const float c = sat * val;
            float h = hue / 60;
            float x = c * (1 - Math.Abs(h % 2 - 1));

            float r = 0;
            float g = 0;
            float b = 0;
            if (h is >= 0 and < 1)       { r = c; g = x; b = 0; }
            else if (h is >= 1 and < 2)  { r = x; g = c; b = 0; }
            else if (h is >= 2 and < 3)  { r = 0; g = c; b = x; }
            else if (h is >= 3 and < 4)  { r = 0; g = x; b = c; }
            else if (h is >= 4 and < 5)  { r = x; g = 0; b = c; }
            else if (h is >= 5 and <= 6) { r = c; g = 0; b = x; }

            const float m = val - c;
            byte red = (byte)Math.Round(ConvertRange(0, 1, 0, 255, r + m));
            byte green = (byte)Math.Round(ConvertRange(0, 1, 0, 255, g + m));
            byte blue = (byte)Math.Round(ConvertRange(0, 1, 0, 255, b + m));

            return (uint)(0xFF << 24 | red << 16 | green << 8 | blue);
        }

        public static Task CleanTempFolders()
        {
            return Task.Run(() =>
            {
                IEnumerable<string> tempFiles = Directory.EnumerateFiles(Paths.TempFolder, "*", SearchOption.AllDirectories);
                IEnumerable<string> logFiles = Directory.EnumerateFiles(Paths.LogFolder, "*", SearchOption.AllDirectories);
                IEnumerable<string> files = tempFiles.Concat(logFiles).Where(f => f != Paths.Log);
                
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            });
        }
    }
}
