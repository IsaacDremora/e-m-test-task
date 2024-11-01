﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace e_m_test_task.Migrations
{
    [DbContext(typeof(AppContext))]
    [Migration("20241023085434_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("District", b =>
                {
                    b.Property<int>("DistrictId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DistrictId"));

                    b.Property<string>("DistrictName")
                        .HasColumnType("text");

                    b.HasKey("DistrictId")
                        .HasName("PrimaryKey_DisctrictId");

                    b.ToTable("Districts");
                });

            modelBuilder.Entity("Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("OrderId"));

                    b.Property<DateTime>("DeliveryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DistrictId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ExpectedDeliveryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("OrderTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<float>("Weight")
                        .HasColumnType("real");

                    b.HasKey("OrderId");

                    b.ToTable("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
