using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data
{
    [Table("Settings")]
    public class Settings
    {
        [Column("ID"), Key, DefaultValue(1)]
        public uint InternalID { get; set; } = 1;

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

        public Settings()
        {
            if (OperatingSystem.IsMacOS())
            {
                BlenderPath = "/Applications/Blender/blender.app/Contents/MacOS/blender";
            }
            else
            {
                BlenderPath = "";
            }

            BlenderOptionalArguments = "";
            OutputPath = "";
            CurrentGame = Game.Undefined;
            RenderScale = RenderScale.x1;
            AssetsPath = "";
        }
    }
}
