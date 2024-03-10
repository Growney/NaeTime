﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NaeTime.OpenPractice.SQLite.Context;

#nullable disable

namespace NaeTime.OpenPractice.SQLite.Migrations
{
    [DbContext(typeof(NaeTimeDbContext))]
    partial class NaeTimeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.ActiveSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.Property<int>("SessionType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ActiveSession");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.ActiveTimings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte>("Lane")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("LapNumber")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ActiveTimings");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.ConsecutiveLapLeaderboard", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<uint>("ConsecutiveLaps")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ConsecutiveLapLeaderboards");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.Detection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("HardwareTime")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsManual")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Lane")
                        .HasColumnType("INTEGER");

                    b.Property<long>("SoftwareTime")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TimerId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UtcTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Detections");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.EthernetLapRF8Channel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("IpAddress")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Port")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("EthernetLapRF8Channels");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.Lane", b =>
                {
                    b.Property<byte>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<byte?>("BandId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RadioFrequencyInMhz")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Lanes");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.OpenPracticeLap", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("FinishedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PilotId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartedUtc")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TotalMilliseconds")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("OpenPracticeLaps");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.OpenPracticeSession", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long?>("MaximumLapMilliseconds")
                        .HasColumnType("INTEGER");

                    b.Property<long>("MinimumLapMilliseconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TrackId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("OpenPracticeSessions");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.Pilot", b =>
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

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.SingleLapLeaderboard", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SingleLapLeaderboards");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.TimerStatus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ConnectionStatusChanged")
                        .HasColumnType("TEXT");

                    b.Property<bool>("WasConnected")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TimerStatuses");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.Track", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte>("AllowedLanes")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MaximumLapMilliseconds")
                        .HasColumnType("INTEGER");

                    b.Property<long>("MinimumLapMilliseconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.ActiveTimings", b =>
                {
                    b.OwnsOne("NaeTime.Persistence.SQLite.Models.ActiveLap", "ActiveLap", b1 =>
                        {
                            b1.Property<Guid>("ActiveTimingsId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .HasColumnType("TEXT");

                            b1.Property<ulong?>("StartedHardwareTime")
                                .HasColumnType("INTEGER");

                            b1.Property<long>("StartedSoftwareTime")
                                .HasColumnType("INTEGER");

                            b1.Property<DateTime>("StartedUtcTime")
                                .HasColumnType("TEXT");

                            b1.HasKey("ActiveTimingsId");

                            b1.ToTable("ActiveTimings");

                            b1.WithOwner()
                                .HasForeignKey("ActiveTimingsId");
                        });

                    b.OwnsOne("NaeTime.Persistence.SQLite.Models.ActiveSplit", "ActiveSplit", b1 =>
                        {
                            b1.Property<Guid>("ActiveTimingsId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .HasColumnType("TEXT");

                            b1.Property<byte>("SplitNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<long>("StartedSoftwareTime")
                                .HasColumnType("INTEGER");

                            b1.Property<DateTime>("StartedUtcTime")
                                .HasColumnType("TEXT");

                            b1.HasKey("ActiveTimingsId");

                            b1.ToTable("ActiveTimings");

                            b1.WithOwner()
                                .HasForeignKey("ActiveTimingsId");
                        });

                    b.Navigation("ActiveLap");

                    b.Navigation("ActiveSplit");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.ConsecutiveLapLeaderboard", b =>
                {
                    b.OwnsMany("NaeTime.Persistence.SQLite.Models.ConsecutiveLapLeaderboardPosition", "Positions", b1 =>
                        {
                            b1.Property<Guid>("ConsecutiveLapLeaderboardId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime>("LastLapCompletionUtc")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("PilotId")
                                .HasColumnType("TEXT");

                            b1.Property<uint>("Position")
                                .HasColumnType("INTEGER");

                            b1.Property<uint>("TotalLaps")
                                .HasColumnType("INTEGER");

                            b1.Property<long>("TotalMilliseconds")
                                .HasColumnType("INTEGER");

                            b1.HasKey("ConsecutiveLapLeaderboardId", "Id");

                            b1.ToTable("ConsecutiveLapLeaderboardPosition");

                            b1.WithOwner()
                                .HasForeignKey("ConsecutiveLapLeaderboardId");

                            b1.OwnsMany("NaeTime.Persistence.SQLite.Models.ConsecutiveLapLeaderboardPositionLap", "IncludedLaps", b2 =>
                                {
                                    b2.Property<Guid>("ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId")
                                        .HasColumnType("TEXT");

                                    b2.Property<Guid>("ConsecutiveLapLeaderboardPositionId")
                                        .HasColumnType("TEXT");

                                    b2.Property<Guid>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("TEXT");

                                    b2.Property<Guid>("LapId")
                                        .HasColumnType("TEXT");

                                    b2.Property<int>("Ordinal")
                                        .HasColumnType("INTEGER");

                                    b2.HasKey("ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId", "ConsecutiveLapLeaderboardPositionId", "Id");

                                    b2.ToTable("ConsecutiveLapLeaderboardPositionLap");

                                    b2.WithOwner()
                                        .HasForeignKey("ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId", "ConsecutiveLapLeaderboardPositionId");
                                });

                            b1.Navigation("IncludedLaps");
                        });

                    b.Navigation("Positions");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.OpenPracticeSession", b =>
                {
                    b.OwnsMany("NaeTime.Persistence.SQLite.Models.PilotLane", "ActiveLanes", b1 =>
                        {
                            b1.Property<Guid>("OpenPracticeSessionId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<byte>("Lane")
                                .HasColumnType("INTEGER");

                            b1.Property<Guid>("PilotId")
                                .HasColumnType("TEXT");

                            b1.HasKey("OpenPracticeSessionId", "Id");

                            b1.ToTable("PilotLane");

                            b1.WithOwner()
                                .HasForeignKey("OpenPracticeSessionId");
                        });

                    b.Navigation("ActiveLanes");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.SingleLapLeaderboard", b =>
                {
                    b.OwnsMany("NaeTime.Persistence.SQLite.Models.SingleLapLeaderboardPosition", "Positions", b1 =>
                        {
                            b1.Property<Guid>("SingleLapLeaderboardId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime>("CompletionUtc")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("LapId")
                                .HasColumnType("TEXT");

                            b1.Property<long>("LapMilliseconds")
                                .HasColumnType("INTEGER");

                            b1.Property<Guid>("PilotId")
                                .HasColumnType("TEXT");

                            b1.Property<uint>("Position")
                                .HasColumnType("INTEGER");

                            b1.HasKey("SingleLapLeaderboardId", "Id");

                            b1.ToTable("SingleLapLeaderboardPosition");

                            b1.WithOwner()
                                .HasForeignKey("SingleLapLeaderboardId");
                        });

                    b.Navigation("Positions");
                });

            modelBuilder.Entity("NaeTime.Persistence.SQLite.Models.Track", b =>
                {
                    b.OwnsMany("NaeTime.Persistence.SQLite.Models.TrackTimer", "Timers", b1 =>
                        {
                            b1.Property<Guid>("TrackId")
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<Guid>("TimerId")
                                .HasColumnType("TEXT");

                            b1.HasKey("TrackId", "Id");

                            b1.ToTable("TrackTimer");

                            b1.WithOwner()
                                .HasForeignKey("TrackId");
                        });

                    b.Navigation("Timers");
                });
#pragma warning restore 612, 618
        }
    }
}
