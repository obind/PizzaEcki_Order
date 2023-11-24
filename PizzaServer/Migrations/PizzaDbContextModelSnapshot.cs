﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PizzaServer;

#nullable disable

namespace PizzaServer.Migrations
{
    [DbContext(typeof(PizzaDbContext))]
    partial class PizzaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("PizzaServer.OrderAssignment", b =>
                {
                    b.Property<Guid>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("BonNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DriverId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("OrderId");

                    b.ToTable("OrderAssignments");
                });

            modelBuilder.Entity("SharedLibrary.Driver", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Drivers");
                });

            modelBuilder.Entity("SharedLibrary.Order", b =>
                {
                    b.Property<Guid>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("BonNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDelivery")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("OrderId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("SharedLibrary.OrderItem", b =>
                {
                    b.Property<int>("OrderItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Epreis")
                        .HasColumnType("REAL");

                    b.Property<string>("Extras")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Gericht")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Gesamt")
                        .HasColumnType("REAL");

                    b.Property<string>("Größe")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("LieferungsArt")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Menge")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Nr")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Uhrzeit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("OrderItemId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("SharedLibrary.OrderItem", b =>
                {
                    b.HasOne("SharedLibrary.Order", null)
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId");
                });

            modelBuilder.Entity("SharedLibrary.Order", b =>
                {
                    b.Navigation("OrderItems");
                });
#pragma warning restore 612, 618
        }
    }
}
