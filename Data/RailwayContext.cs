using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UABackbone_Backend.Models;

public partial class RailwayContext : DbContext
{
    public RailwayContext(DbContextOptions<RailwayContext> options) : base(options)  { }

    public virtual DbSet<BlacklistedUser> BlacklistedUsers { get; set; }
    public virtual DbSet<PendingUser>     PendingUsers     { get; set; }
    public virtual DbSet<LocalUnion>      LocalUnions      { get; set; }
    public virtual DbSet<User>            Users            { get; set; }

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.LocalId, "Users_LocalUnions_FK");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                  .HasMaxLength(255)
                  .HasColumnName("email")
                  .IsUnicode();
            entity.Property(e => e.FirstName)
                  .HasMaxLength(35)
                  .HasColumnName("first_name");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.IsBlacklisted).HasColumnName("is_blacklisted");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.LastName)
                  .HasMaxLength(35)
                  .HasColumnName("last_name");
            entity.Property(e => e.LocalId)
                  .HasColumnName("local_id")
                  .IsRequired();
            entity.Property(e => e.PasswordHash)
                  .HasMaxLength(60)
                  .HasColumnName("password_hash")
                  .IsRequired();
            entity.Property(e => e.Username)
                  .HasMaxLength(55)
                  .HasColumnName("username")
                  .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
