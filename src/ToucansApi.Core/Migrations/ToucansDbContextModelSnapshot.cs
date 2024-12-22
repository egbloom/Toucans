﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ToucansApi.Core.Data;

#nullable disable

namespace ToucansApi.Core.Migrations
{
    [DbContext(typeof(ToucansDbContext))]
    partial class ToucansDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ToucansApi.Core.Models.TodoItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AssignedToId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ListId")
                        .HasColumnType("uuid");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("TodoListId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("AssignedToId");

                    b.HasIndex("TodoListId");

                    b.ToTable("TodoItems", (string)null);
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoList", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime?>("LastModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("TodoLists", (string)null);
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoListShare", b =>
                {
                    b.Property<Guid>("TodoListId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SharedWithUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("TodoListId", "SharedWithUserId");

                    b.HasIndex("SharedWithUserId");

                    b.HasIndex("UserId");

                    b.ToTable("TodoListShares", (string)null);
                });

            modelBuilder.Entity("ToucansApi.Core.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoItem", b =>
                {
                    b.HasOne("ToucansApi.Core.Models.User", "AssignedTo")
                        .WithMany()
                        .HasForeignKey("AssignedToId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("ToucansApi.Core.Models.TodoList", "TodoList")
                        .WithMany("Items")
                        .HasForeignKey("TodoListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AssignedTo");

                    b.Navigation("TodoList");
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoList", b =>
                {
                    b.HasOne("ToucansApi.Core.Models.User", "Owner")
                        .WithMany("OwnedLists")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoListShare", b =>
                {
                    b.HasOne("ToucansApi.Core.Models.User", "SharedWithUser")
                        .WithMany()
                        .HasForeignKey("SharedWithUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ToucansApi.Core.Models.TodoList", "TodoList")
                        .WithMany("Shares")
                        .HasForeignKey("TodoListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ToucansApi.Core.Models.User", null)
                        .WithMany("SharedLists")
                        .HasForeignKey("UserId");

                    b.Navigation("SharedWithUser");

                    b.Navigation("TodoList");
                });

            modelBuilder.Entity("ToucansApi.Core.Models.TodoList", b =>
                {
                    b.Navigation("Items");

                    b.Navigation("Shares");
                });

            modelBuilder.Entity("ToucansApi.Core.Models.User", b =>
                {
                    b.Navigation("OwnedLists");

                    b.Navigation("SharedLists");
                });
#pragma warning restore 612, 618
        }
    }
}
