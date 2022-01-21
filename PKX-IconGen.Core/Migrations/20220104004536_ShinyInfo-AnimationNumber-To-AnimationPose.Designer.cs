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
    [Migration("20220104004536_ShinyInfo-AnimationNumber-To-AnimationPose")]
    partial class ShinyInfoAnimationNumberToAnimationPose
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("PKXIconGen.Core.Data.PokemonRenderData", b =>
                {
                    b.Property<uint>("InternalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("ID");

                    b.Property<ushort>("AnimationFrame")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("AnimationPose")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BuiltIn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MainCamera")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("MainLights")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SecondaryCamera")
                        .HasColumnType("TEXT");

                    b.Property<string>("SecondaryLights")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Shiny")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("InternalID");

                    b.HasIndex(new[] { "InternalID" }, "IDX_ID")
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
                            OutputPath = "",
                            RenderScale = (byte)0
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
