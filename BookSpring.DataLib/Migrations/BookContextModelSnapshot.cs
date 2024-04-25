﻿// <auto-generated />
using BookSpring.DataLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BookSpring.DataLib.Migrations
{
    [DbContext(typeof(BookContext))]
    partial class BookContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("BookSpring.DataLib.DataModels.BookModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("varchar(10)");

                    b.Property<string>("CreatedById")
                        .HasColumnType("varchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("varchar(10)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("IsEBook")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LendDate")
                        .HasColumnType("varchar(10)");

                    b.Property<string>("LendToId")
                        .IsRequired()
                        .HasColumnType("varchar(10)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ReturnDate")
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("LendToId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("BookSpring.DataLib.DataModels.UserModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(10)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(15)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BookSpring.DataLib.DataModels.BookModel", b =>
                {
                    b.HasOne("BookSpring.DataLib.DataModels.UserModel", "CreatedBy")
                        .WithMany("CreatedBooks")
                        .HasForeignKey("CreatedById");

                    b.HasOne("BookSpring.DataLib.DataModels.UserModel", "LendTo")
                        .WithMany("LendBooks")
                        .HasForeignKey("LendToId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("LendTo");
                });

            modelBuilder.Entity("BookSpring.DataLib.DataModels.UserModel", b =>
                {
                    b.Navigation("CreatedBooks");

                    b.Navigation("LendBooks");
                });
#pragma warning restore 612, 618
        }
    }
}
