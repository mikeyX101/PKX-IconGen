﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PKXIconGen.Core.Services;

namespace PKXIconGen.Core.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20211214032611_Output-Path")]
    partial class OutputPath
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.12");

            modelBuilder.Entity("PKXIconGen.Models.Settings", b =>
                {
                    b.Property<uint>("InternalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("ID");

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
