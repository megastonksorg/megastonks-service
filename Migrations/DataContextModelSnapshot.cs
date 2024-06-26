﻿// <auto-generated />
using System;
using Megastonks.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Megastonks.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Megastonks.Entities.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("AcceptTerms")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DeviceToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DeviceType")
                        .HasColumnType("nvarchar(24)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsOnboarded")
                        .HasColumnType("bit");

                    b.Property<string>("ProfilePhoto")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublicKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(24)");

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Verified")
                        .HasColumnType("datetime2");

                    b.Property<string>("WalletAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("PublicKey")
                        .IsUnique()
                        .HasFilter("[PublicKey] IS NOT NULL");

                    b.HasIndex("WalletAddress")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Megastonks.Entities.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Caption")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ContextId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SenderId")
                        .HasColumnType("int");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("nvarchar(24)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("TribeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(24)");

                    b.HasKey("Id");

                    b.HasIndex("ContextId");

                    b.HasIndex("SenderId");

                    b.HasIndex("TribeId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Megastonks.Entities.Tribe", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TimestampId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Tribes");
                });

            modelBuilder.Entity("Megastonks.Entities.TribeInviteCode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("TribeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("TribeId");

                    b.ToTable("TribeInviteCodes");
                });

            modelBuilder.Entity("Megastonks.Entities.Account", b =>
                {
                    b.OwnsMany("Megastonks.Entities.RefreshToken", "RefreshTokens", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int>("AccountId")
                                .HasColumnType("int");

                            b1.Property<DateTime>("Created")
                                .HasColumnType("datetime2");

                            b1.Property<string>("CreatedByIp")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime>("Expires")
                                .HasColumnType("datetime2");

                            b1.Property<string>("ReplacedByToken")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime?>("Revoked")
                                .HasColumnType("datetime2");

                            b1.Property<string>("RevokedByIp")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Token")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("Id");

                            b1.HasIndex("AccountId");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner("Account")
                                .HasForeignKey("AccountId");

                            b1.Navigation("Account");
                        });

                    b.Navigation("RefreshTokens");
                });

            modelBuilder.Entity("Megastonks.Entities.Message", b =>
                {
                    b.HasOne("Megastonks.Entities.Message", "Context")
                        .WithMany()
                        .HasForeignKey("ContextId");

                    b.HasOne("Megastonks.Entities.Account", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.HasOne("Megastonks.Entities.Tribe", "Tribe")
                        .WithMany()
                        .HasForeignKey("TribeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Megastonks.Entities.MessageKey", "Keys", b1 =>
                        {
                            b1.Property<Guid>("MessageId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<string>("EncryptionKey")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("PublicKey")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("MessageId", "Id");

                            b1.ToTable("MessageKey");

                            b1.WithOwner("Message")
                                .HasForeignKey("MessageId");

                            b1.Navigation("Message");
                        });

                    b.OwnsMany("Megastonks.Entities.MessageReaction", "Reactions", b1 =>
                        {
                            b1.Property<Guid>("MessageId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<string>("Content")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<int>("SenderId")
                                .HasColumnType("int");

                            b1.HasKey("MessageId", "Id");

                            b1.HasIndex("SenderId");

                            b1.ToTable("MessageReaction");

                            b1.WithOwner("Message")
                                .HasForeignKey("MessageId");

                            b1.HasOne("Megastonks.Entities.Account", "Sender")
                                .WithMany()
                                .HasForeignKey("SenderId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.Navigation("Message");

                            b1.Navigation("Sender");
                        });

                    b.OwnsMany("Megastonks.Entities.MessageViewer", "Viewers", b1 =>
                        {
                            b1.Property<Guid>("MessageId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int>("AccountId")
                                .HasColumnType("int");

                            b1.HasKey("MessageId", "Id");

                            b1.HasIndex("AccountId");

                            b1.ToTable("MessageViewer");

                            b1.HasOne("Megastonks.Entities.Account", "Account")
                                .WithMany()
                                .HasForeignKey("AccountId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner("Message")
                                .HasForeignKey("MessageId");

                            b1.Navigation("Account");

                            b1.Navigation("Message");
                        });

                    b.Navigation("Context");

                    b.Navigation("Keys");

                    b.Navigation("Reactions");

                    b.Navigation("Sender");

                    b.Navigation("Tribe");

                    b.Navigation("Viewers");
                });

            modelBuilder.Entity("Megastonks.Entities.Tribe", b =>
                {
                    b.OwnsMany("Megastonks.Entities.TribeMember", "TribeMembers", b1 =>
                        {
                            b1.Property<Guid>("TribeId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int>("AccountId")
                                .HasColumnType("int");

                            b1.Property<DateTime>("Joined")
                                .HasColumnType("datetime2");

                            b1.HasKey("TribeId", "Id");

                            b1.HasIndex("AccountId");

                            b1.ToTable("TribeMember");

                            b1.HasOne("Megastonks.Entities.Account", "Account")
                                .WithMany()
                                .HasForeignKey("AccountId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner("Tribe")
                                .HasForeignKey("TribeId");

                            b1.Navigation("Account");

                            b1.Navigation("Tribe");
                        });

                    b.Navigation("TribeMembers");
                });

            modelBuilder.Entity("Megastonks.Entities.TribeInviteCode", b =>
                {
                    b.HasOne("Megastonks.Entities.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Megastonks.Entities.Tribe", "Tribe")
                        .WithMany()
                        .HasForeignKey("TribeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Tribe");
                });
#pragma warning restore 612, 618
        }
    }
}
