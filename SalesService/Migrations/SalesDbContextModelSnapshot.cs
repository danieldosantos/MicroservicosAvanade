using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesService.Data;
using SalesService.Models;

#nullable disable

namespace SalesService.Migrations
{
    [DbContext(typeof(SalesDbContext))]
    partial class SalesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity<Order>(b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime2");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("Orders");
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<int>("OrderId")
                    .HasColumnType("int");

                b.Property<int>("ProductId")
                    .HasColumnType("int");

                b.Property<int>("Quantity")
                    .HasColumnType("int");

                b.Property<decimal>("UnitPrice")
                    .HasColumnType("decimal(18,2)");

                b.HasKey("Id");

                b.HasIndex("OrderId");

                b.ToTable("OrderItems");
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.HasOne<Order>("Order")
                    .WithMany("Items")
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.Navigation("Items");
            });
        }
    }
}

