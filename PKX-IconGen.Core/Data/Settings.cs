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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PKXIconGen.Core.Data
{
    [Table("Settings")]
    public class Settings
    {
        [Column("ID"), Key, DefaultValue(1)]
        public uint InternalID { get; private set; } = 1;

        [Column]
        public string BlenderPath { get; set; }
        [Column]
        public string BlenderOptionalArguments { get; set; }
        [Column]
        public string OutputPath { get; set; }
        [Column]
        public Game CurrentGame { get; set; }
        [Column]
        public RenderScale RenderScale { get; set; }
        [Column]
        public string AssetsPath { get; set; }
        
        [Column]
        public bool LogBlender { get; set; }
        
        [Column, DefaultValue(true)]
        public bool SaturationBoost { get; set; }
        
        [Column, DefaultValue(false)]
        public bool SaveDanceGIF { get; set; }

        public Settings()
        {
            BlenderPath = OperatingSystem.IsMacOS() ? "/Applications/Blender/blender.app/Contents/MacOS/blender" : "";

            BlenderOptionalArguments = "";
            OutputPath = "";
            CurrentGame = Game.Undefined;
            RenderScale = RenderScale.X1;
            AssetsPath = "";
            
            LogBlender = false;
            SaturationBoost = true;
            SaveDanceGIF = false;
        }
    }
}
