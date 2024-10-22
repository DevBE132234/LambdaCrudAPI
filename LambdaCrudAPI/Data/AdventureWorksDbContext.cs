using System;
using System.Collections.Generic;
using LambdaCrudAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LambdaCrudAPI.Data;

public partial class AdventureWorksDbContext : DbContext
{
    public AdventureWorksDbContext()
    {
    }

    public AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source =DESKTOP-1T8I7CE\\SQLEXPRESS01;Initial Catalog=AdventureWorks2022; Integrated Security=true; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK_Product_ProductID");

            entity.ToTable("Product", "Production", tb => tb.HasComment("Products sold or used in the manfacturing of sold products."));

            entity.Property(e => e.ProductId).HasComment("Primary key for Product records.");
            entity.Property(e => e.Class)
                .IsFixedLength()
                .HasComment("H = High, M = Medium, L = Low");
            entity.Property(e => e.Color).HasComment("Product color.");
            entity.Property(e => e.DaysToManufacture).HasComment("Number of days required to manufacture the product.");
            entity.Property(e => e.DiscontinuedDate).HasComment("Date the product was discontinued.");
            entity.Property(e => e.FinishedGoodsFlag)
                .HasDefaultValue(true)
                .HasComment("0 = Product is not a salable item. 1 = Product is salable.");
            entity.Property(e => e.ListPrice).HasComment("Selling price.");
            entity.Property(e => e.MakeFlag)
                .HasDefaultValue(true)
                .HasComment("0 = Product is purchased, 1 = Product is manufactured in-house.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("Name of the product.");
            entity.Property(e => e.ProductLine)
                .IsFixedLength()
                .HasComment("R = Road, M = Mountain, T = Touring, S = Standard");
            entity.Property(e => e.ProductModelId).HasComment("Product is a member of this product model. Foreign key to ProductModel.ProductModelID.");
            entity.Property(e => e.ProductNumber).HasComment("Unique product identification number.");
            entity.Property(e => e.ProductSubcategoryId).HasComment("Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID. ");
            entity.Property(e => e.ReorderPoint).HasComment("Inventory level that triggers a purchase order or work order. ");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            entity.Property(e => e.SafetyStockLevel).HasComment("Minimum inventory quantity. ");
            entity.Property(e => e.SellEndDate).HasComment("Date the product was no longer available for sale.");
            entity.Property(e => e.SellStartDate).HasComment("Date the product was available for sale.");
            entity.Property(e => e.Size).HasComment("Product size.");
            entity.Property(e => e.SizeUnitMeasureCode)
                .IsFixedLength()
                .HasComment("Unit of measure for Size column.");
            entity.Property(e => e.StandardCost).HasComment("Standard cost of the product.");
            entity.Property(e => e.Style)
                .IsFixedLength()
                .HasComment("W = Womens, M = Mens, U = Universal");
            entity.Property(e => e.Weight).HasComment("Product weight.");
            entity.Property(e => e.WeightUnitMeasureCode)
                .IsFixedLength()
                .HasComment("Unit of measure for Weight column.");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
