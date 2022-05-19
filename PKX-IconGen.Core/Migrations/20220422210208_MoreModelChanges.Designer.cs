﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PKXIconGen.Core.Services;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220422210208_MoreModelChanges")]
    partial class MoreModelChanges
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("PKXIconGen.Core.Data.PokemonRenderData", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("ID");

                    b.Property<bool>("BuiltIn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OutputName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Render")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}");

                    b.Property<string>("Shiny")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}}");

                    b.HasKey("ID");

                    b.HasIndex(new[] { "ID" }, "IDX_ID")
                        .IsUnique();

                    b.ToTable("PokemonRenderData");
                });

            modelBuilder.Entity("PKXIconGen.Core.Data.Settings", b =>
                {
                    b.Property<uint>("InternalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("ID");

                    b.Property<string>("AssetsPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BlenderOptionalArguments")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BlenderPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte>("CurrentGame")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LogBlender")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OutputPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte>("RenderScale")
                        .HasColumnType("INTEGER");

                    b.HasKey("InternalID");

                    b.ToTable("Settings");

                    b.HasData(
                        new
                        {
                            InternalID = 1u,
                            AssetsPath = "",
                            BlenderOptionalArguments = "",
                            BlenderPath = "",
                            CurrentGame = (byte)0,
                            LogBlender = false,
                            OutputPath = "",
                            RenderScale = (byte)1
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
