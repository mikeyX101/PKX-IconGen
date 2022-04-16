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
    [Migration("20220413235853_temp")]
    partial class temp
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

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
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RemovedObjects")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("[]");

                    b.Property<string>("Render")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}");

                    b.Property<string>("Shiny")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{\"filter\":null,\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}}");

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
                            RenderScale = (byte)0
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
