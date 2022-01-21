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

using PKXIconGen.Core.Data.Blender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
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

            float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
            return newStart + ((value - originalStart) * scale);
        }
    }
}
