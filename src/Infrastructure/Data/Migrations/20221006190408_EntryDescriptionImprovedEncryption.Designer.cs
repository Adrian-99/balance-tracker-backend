﻿// <auto-generated />
using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221006190408_EntryDescriptionImprovedEncryption")]
    partial class EntryDescriptionImprovedEncryption
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IconColor")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsIncome")
                        .HasColumnType("boolean");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("OrderOnList")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Keyword")
                        .IsUnique();

                    b.HasIndex("OrderOnList")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Domain.Entities.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DescriptionContent")
                        .HasColumnType("text");

                    b.Property<string>("DescriptionIV")
                        .HasColumnType("text");

                    b.Property<string>("DescriptionKey")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("Domain.Entities.EntryTag", b =>
                {
                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EntryId")
                        .HasColumnType("uuid");

                    b.HasKey("TagId", "EntryId")
                        .HasName("PK_EntryTags");

                    b.HasIndex("EntryId");

                    b.ToTable("EntryTags");
                });

            modelBuilder.Entity("Domain.Entities.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmailVerificationCode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EmailVerificationCodeCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<bool>("IsEmailVerified")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUsernameChangeAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("ResetPasswordCode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ResetPasswordCodeCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("EmailVerificationCode")
                        .IsUnique();

                    b.HasIndex("ResetPasswordCode")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Domain.Entities.Entry", b =>
                {
                    b.HasOne("Domain.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany("Entries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.EntryTag", b =>
                {
                    b.HasOne("Domain.Entities.Entry", "Entry")
                        .WithMany("EntryTags")
                        .HasForeignKey("EntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Tag", "Tag")
                        .WithMany("EntryTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entry");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Domain.Entities.Tag", b =>
                {
                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany("Tags")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Entry", b =>
                {
                    b.Navigation("EntryTags");
                });

            modelBuilder.Entity("Domain.Entities.Tag", b =>
                {
                    b.Navigation("EntryTags");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Navigation("Entries");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
