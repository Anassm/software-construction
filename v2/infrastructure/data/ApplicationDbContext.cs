using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using v2.Core.Models;

namespace ChefServe.Infrastructure.Data
{
      public class ApplicationDbContext : IdentityDbContext<IdentityUser>
      {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            { }

            public DbSet<User> Users { get; set; }
            public DbSet<Vehicle> Vehicles { get; set; }
            public DbSet<Session> Sessions { get; set; }
            public DbSet<Reservation> Reservations { get; set; }
            public DbSet<Payment> Payments { get; set; }
            public DbSet<ParkingLot> ParkingLots { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  modelBuilder.Entity<User>(entity =>
                  {
                        entity.HasKey(u => u.ID);
                        entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                        entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
                        entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                        entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
                        entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                        entity.Property(u => u.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.Property(u => u.BirthDate).IsRequired();
                        entity.Property(u => u.IsActive).HasDefaultValueSql("1");
                        modelBuilder.Entity<User>()
                        .HasOne(u => u.IdentityUser)
                        .WithOne()
                        .HasForeignKey<User>(u => u.IdentityUserId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<Vehicle>(entity =>
                  {
                        entity.HasKey(v => v.ID);
                        entity.Property(v => v.LicensePlate)
                        .IsRequired()
                        .HasMaxLength(20);
                        entity.Property(v => v.Make)
                        .IsRequired().HasMaxLength(50);
                        entity.Property(v => v.Model)
                        .IsRequired()
                        .HasMaxLength(50);
                        entity.Property(v => v.Color)
                        .IsRequired()
                        .HasMaxLength(30);
                        entity.Property(v => v.Year).IsRequired();
                        entity.Property(v => v.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.HasOne<User>()
                        .WithMany(u => u.Vehicles)
                        .HasForeignKey(v => v.UserID)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<ParkingLot>(entity =>
                  {
                        entity.HasKey(p => p.ID);
                        entity.Property(p => p.Name)
                        .IsRequired()
                        .HasMaxLength(100);
                        entity.Property(p => p.Location)
                        .IsRequired()
                        .HasMaxLength(150);
                        entity.Property(p => p.Address)
                        .IsRequired()
                        .HasMaxLength(250);
                        entity.Property(p => p.Capacity).IsRequired();
                        entity.Property(p => p.Reserved).IsRequired();
                        entity.Property(p => p.Tariff).IsRequired();
                        entity.Property(p => p.DayTariff).IsRequired();
                        entity.Property(p => p.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.Property(p => p.latitude).IsRequired();
                        entity.Property(p => p.longitude).IsRequired();
                  });

                  modelBuilder.Entity<Reservation>(entity =>
                  {
                        entity.HasKey(r => r.ID);
                        entity.Property(r => r.StartDate).IsRequired();
                        entity.Property(r => r.EndDate).IsRequired();
                        entity.Property(r => r.Status).IsRequired().HasMaxLength(50);
                        entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.Property(r => r.TotalPrice).IsRequired();

                        entity.HasOne(r => r.User)
                        .WithMany(u => u.Reservations)
                        .HasForeignKey(r => r.UserID)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(r => r.ParkingLot)
                        .WithMany(p => p.Reservations)
                        .HasForeignKey(r => r.ParkingLotID)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(r => r.Vehicle)
                        .WithMany(v => v.Reservations)
                        .HasForeignKey(r => r.VehicleID)
                        .OnDelete(DeleteBehavior.Cascade);
                  });


                  modelBuilder.Entity<Session>(entity =>
                  {
                        entity.HasKey(s => s.ID);
                        entity.Property(s => s.LicensePlate).IsRequired().HasMaxLength(20);
                        entity.Property(s => s.StartTime).IsRequired();
                        entity.Property(s => s.EndTime).IsRequired(false);
                        entity.Property(s => s.duration).IsRequired();
                        entity.Property(s => s.Price).IsRequired().HasDefaultValue(0);
                        entity.Property(s => s.PaymentStatus).IsRequired().HasDefaultValue(PaymentStatus.Unpaid);

                        entity.HasOne(s => s.User)
                        .WithMany(u => u.Sessions)
                        .HasForeignKey(s => s.UserID)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(s => s.ParkingLot)
                        .WithMany(p => p.Sessions)
                        .HasForeignKey(s => s.ParkingLotID)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  modelBuilder.Entity<Payment>(entity =>
                  {
                        entity.HasKey(p => p.ID);
                        entity.Property(p => p.Amount).IsRequired();
                        entity.Property(p => p.Initiator).IsRequired().HasMaxLength(100);
                        entity.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                        entity.Property(p => p.CompletedAt).IsRequired();
                        entity.Property(p => p.Hash).HasMaxLength(256);
                        entity.Property(p => p.TransactionAmount).IsRequired();
                        entity.Property(p => p.TransactionDate).IsRequired();
                        entity.Property(p => p.TransactionMethod).IsRequired().HasMaxLength(50);
                        entity.Property(p => p.TransactionIssuer).IsRequired().HasMaxLength(100);
                        entity.Property(p => p.TransactionBank).IsRequired().HasMaxLength(100);

                        entity.HasOne(p => p.Session)
                        .WithOne(s => s.Payment)
                        .HasForeignKey<Payment>(p => p.SessionID)
                        .OnDelete(DeleteBehavior.Cascade);
                  });
            }
      }
}