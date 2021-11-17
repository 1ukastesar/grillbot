﻿// <auto-generated />
using System;
using System.Collections.Generic;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GrillBot.Database.Migrations
{
    [DbContext(typeof(GrillBotContext))]
    [Migration("20211117190453_RemovedApiToken")]
    partial class RemovedApiToken
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("GrillBot.Database.Entity.AuditLogFileMeta", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("AuditLogItemId")
                        .HasColumnType("bigint");

                    b.Property<string>("Filename")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("AuditLogItemId");

                    b.ToTable("AuditLogFiles");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.AuditLogItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .HasColumnType("text");

                    b.Property<string>("DiscordAuditLogItemId")
                        .HasColumnType("text");

                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ProcessedUserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GuildId", "ChannelId");

                    b.HasIndex("GuildId", "ProcessedUserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteStatisticItem", b =>
                {
                    b.Property<string>("EmoteId")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime>("FirstOccurence")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastOccurence")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("UseCount")
                        .HasColumnType("bigint");

                    b.HasKey("EmoteId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Emotes");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.ExplicitPermission", b =>
                {
                    b.Property<string>("Command")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("TargetId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<bool>("IsRole")
                        .HasColumnType("boolean");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.HasKey("Command", "TargetId");

                    b.ToTable("ExplicitPermissions");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Guild", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("AdminChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("BoosterRoleId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("MuteRoleId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("ChannelType")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<long>("GivenReactions")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("LastPointsMessageIncrement")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("LastPointsReactionIncrement")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Nickname")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<long>("ObtainedReactions")
                        .HasColumnType("bigint");

                    b.Property<long>("Points")
                        .HasColumnType("bigint");

                    b.Property<string>("UsedInviteCode")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("UsedInviteCode");

                    b.HasIndex("UserId");

                    b.ToTable("GuildUsers");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUserChannel", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("FirstMessageAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastMessageAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GuildId", "Id", "UserId");

                    b.HasIndex("UserId");

                    b.HasIndex("GuildId", "UserId");

                    b.ToTable("UserChannels");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.Property<string>("Code")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Code");

                    b.HasIndex("GuildId", "CreatorId");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.RemindMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("At")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginalMessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Postpone")
                        .HasColumnType("integer");

                    b.Property<string>("RemindMessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ToUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ToUserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SearchItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("JumpUrl")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("MessageContent")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<string>("MessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("GuildId", "ChannelId");

                    b.ToTable("SearchItems");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SelfunverifyKeepable", b =>
                {
                    b.Property<string>("GroupName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("GroupName", "Name");

                    b.ToTable("SelfunverifyKeepables");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Unverify", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<List<GuildChannelOverride>>("Channels")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("EndAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<List<string>>("Roles")
                        .HasColumnType("jsonb");

                    b.Property<long>("SetOperationId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("SetOperationId")
                        .IsUnique();

                    b.ToTable("Unverifies");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .HasColumnType("text");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Operation")
                        .HasColumnType("integer");

                    b.Property<string>("ToUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId", "FromUserId");

                    b.HasIndex("GuildId", "ToUserId");

                    b.ToTable("UnverifyLogs");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Flags")
                        .HasColumnType("integer");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<string>("SelfUnverifyMinimalTime")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.AuditLogFileMeta", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.AuditLogItem", "AuditLogItem")
                        .WithMany("Files")
                        .HasForeignKey("AuditLogItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuditLogItem");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.AuditLogItem", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("AuditLogs")
                        .HasForeignKey("GuildId");

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "GuildChannel")
                        .WithMany()
                        .HasForeignKey("GuildId", "ChannelId");

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "ProcessedGuildUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "ProcessedUserId");

                    b.Navigation("Guild");

                    b.Navigation("GuildChannel");

                    b.Navigation("ProcessedGuildUser");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteStatisticItem", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.User", "User")
                        .WithMany("UsedEmotes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.Invite", "UsedInvite")
                        .WithMany("UsedUsers")
                        .HasForeignKey("UsedInviteCode");

                    b.HasOne("GrillBot.Database.Entity.User", "User")
                        .WithMany("Guilds")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("UsedInvite");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUserChannel", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", null)
                        .WithMany("Channels")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "Channel")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "User")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Channel");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Invites")
                        .HasForeignKey("GuildId");

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "Creator")
                        .WithMany("CreatedInvites")
                        .HasForeignKey("GuildId", "CreatorId");

                    b.Navigation("Creator");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.RemindMessage", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.User", "FromUser")
                        .WithMany("OutgoingReminders")
                        .HasForeignKey("FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", "ToUser")
                        .WithMany("IncomingReminders")
                        .HasForeignKey("ToUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SearchItem", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Searches")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", "User")
                        .WithMany("SearchItems")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "Channel")
                        .WithMany("SearchItems")
                        .HasForeignKey("GuildId", "ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Channel");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Unverify", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Unverifies")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.UnverifyLog", "UnverifyLog")
                        .WithOne("Unverify")
                        .HasForeignKey("GrillBot.Database.Entity.Unverify", "SetOperationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "GuildUser")
                        .WithOne("Unverify")
                        .HasForeignKey("GrillBot.Database.Entity.Unverify", "GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("GuildUser");

                    b.Navigation("UnverifyLog");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("UnverifyLogs")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "FromUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "ToUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "ToUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("Guild");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.AuditLogItem", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Guild", b =>
                {
                    b.Navigation("AuditLogs");

                    b.Navigation("Channels");

                    b.Navigation("Invites");

                    b.Navigation("Searches");

                    b.Navigation("Unverifies");

                    b.Navigation("UnverifyLogs");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("SearchItems");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.Navigation("CreatedInvites");

                    b.Navigation("Channels");

                    b.Navigation("Unverify");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.Navigation("UsedUsers");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.Navigation("Unverify");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.User", b =>
                {
                    b.Navigation("Guilds");

                    b.Navigation("Channels");

                    b.Navigation("IncomingReminders");

                    b.Navigation("OutgoingReminders");

                    b.Navigation("SearchItems");

                    b.Navigation("UsedEmotes");
                });
#pragma warning restore 612, 618
        }
    }
}
