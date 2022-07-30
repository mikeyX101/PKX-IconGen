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
    [Migration("20220730214239_OrthographicCamera")]
    partial class OrthographicCamera
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.7");

            modelBuilder.Entity("PKXIconGen.Core.Data.PokemonRenderData", b =>
                {
                    b.Property<uint>("Id")
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
                        .HasDefaultValue("{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}");

                    b.Property<string>("Shiny")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "IDX_ID")
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
