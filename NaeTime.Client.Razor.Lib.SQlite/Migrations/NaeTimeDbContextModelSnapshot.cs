﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NaeTime.Client.Razor.Lib.SQlite;

#nullable disable

namespace NaeTime.Client.Razor.Lib.SQlite.Migrations
{
    [DbContext(typeof(NaeTimeDbContext))]
    partial class NaeTimeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("NaeTime.Client.Razor.Lib.SQlite.Models.EthernetLapRF8ChannelDetails", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("EthernetLapRF8Channels");
                });

            modelBuilder.Entity("NaeTime.Client.Razor.Lib.SQlite.Models.FlyingSessionDetails", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpectedEnd")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TrackId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FlyingSessions");
                });

            modelBuilder.Entity("NaeTime.Client.Razor.Lib.SQlite.Models.PilotDetails", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CallSign")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Pilots");
                });

            modelBuilder.Entity("NaeTime.Client.Razor.Lib.SQlite.Models.TrackDetails", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("NaeTime.Client.Razor.Lib.SQlite.Models.TrackDetails", b =>
                {
                    b.OwnsMany("NaeTime.Client.Razor.Lib.SQlite.Models.TimedGateDetails", "TimedGates", b1 =>
                        {
                            b1.Property<Guid>("TrackDetailsId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("INTEGER");

                            b1.Property<Guid>("TimerId")
                                .HasColumnType("TEXT");

                            b1.HasKey("TrackDetailsId", "Id");

                            b1.ToTable("TimedGates");

                            b1.WithOwner()
                                .HasForeignKey("TrackDetailsId");
                        });

                    b.Navigation("TimedGates");
                });
#pragma warning restore 612, 618
        }
    }
}
