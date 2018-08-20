﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiraiNotes.Data;

namespace MiraiNotes.Data.Migrations
{
    [DbContext(typeof(MiraiNotesContext))]
    [Migration("20180820174100_SecondMigration")]
    partial class SecondMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("MiraiNotes.Data.Models.GoogleTask", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CompletedOn");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("GoogleTaskID")
                        .IsRequired();

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsHidden");

                    b.Property<int>("LocalStatus");

                    b.Property<string>("Notes");

                    b.Property<string>("ParentTask");

                    b.Property<string>("Position");

                    b.Property<string>("Status")
                        .IsRequired();

                    b.Property<int?>("TaskListID");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<DateTimeOffset?>("ToBeCompletedOn");

                    b.Property<bool>("ToBeSynced");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("ID");

                    b.HasIndex("GoogleTaskID")
                        .IsUnique();

                    b.HasIndex("TaskListID");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("MiraiNotes.Data.Models.GoogleTaskList", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("GoogleTaskListID")
                        .IsRequired();

                    b.Property<int>("LocalStatus");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<bool>("ToBeSynced");

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<int?>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("GoogleTaskListID")
                        .IsUnique();

                    b.HasIndex("UserID");

                    b.ToTable("TaskLists");
                });

            modelBuilder.Entity("MiraiNotes.Data.Models.GoogleUser", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("Fullname")
                        .IsRequired();

                    b.Property<string>("GoogleUserID")
                        .IsRequired();

                    b.Property<bool>("IsActive");

                    b.Property<string>("PictureUrl");

                    b.HasKey("ID");

                    b.HasIndex("GoogleUserID")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MiraiNotes.Data.Models.GoogleTask", b =>
                {
                    b.HasOne("MiraiNotes.Data.Models.GoogleTaskList", "TaskList")
                        .WithMany("Tasks")
                        .HasForeignKey("TaskListID");
                });

            modelBuilder.Entity("MiraiNotes.Data.Models.GoogleTaskList", b =>
                {
                    b.HasOne("MiraiNotes.Data.Models.GoogleUser", "User")
                        .WithMany("TaskLists")
                        .HasForeignKey("UserID");
                });
#pragma warning restore 612, 618
        }
    }
}
