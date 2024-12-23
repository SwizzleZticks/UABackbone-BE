using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UABackbone_Backend.Models;

public partial class RailwayContext : DbContext
{
    public RailwayContext(DbContextOptions<RailwayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LocalUnion> LocalUnions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<LocalUnion>(entity =>
        {
            entity.HasKey(e => e.Local).HasName("PRIMARY");

            entity.Property(e => e.Local).ValueGeneratedNever();
            entity.Property(e => e.Annuity).HasPrecision(5, 2);
            entity.Property(e => e.HealthWelfare).HasPrecision(5, 2);
            entity.Property(e => e.LocalPension).HasPrecision(5, 2);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.NationalPension).HasPrecision(5, 2);
            entity.Property(e => e.Vacation).HasPrecision(5, 2);
            entity.Property(e => e.Wage).HasPrecision(5, 2);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
