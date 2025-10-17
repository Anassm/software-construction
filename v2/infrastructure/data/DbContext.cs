// using Microsoft.EntityFrameworkCore;
// using ChefServe.Core.Models;

// namespace ChefServe.Infrastructure.Data
// {
//     public class ChefServeDbContext : DbContext
//     {
//         public ChefServeDbContext(DbContextOptions<ChefServeDbContext> options)
//             : base(options)
//         { }

//         public DbSet<User> Users { get; set; }
//         public DbSet<FileItem> FileItems { get; set; }
//         public DbSet<SharedFileItem> SharedFileItems { get; set; }
//         public DbSet<Session> Sessions { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             modelBuilder.Entity<User>(entity =>
//             {
//                 entity.HasKey(u => u.ID);
//                 entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
//                 entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
//                 entity.Property(u => u.CreatedAt)
//                       .HasDefaultValueSql("CURRENT_TIMESTAMP");
//             });

//             modelBuilder.Entity<FileItem>(entity =>
//             {
//                 entity.HasKey(f => f.ID);
//                 entity.Property(f => f.Name).IsRequired().HasMaxLength(255);
//                 entity.Property(f => f.Path).IsRequired();
//                 entity.Property(f => f.Extension).HasMaxLength(255);
//                 entity.Property(f => f.ParentPath).IsRequired();
//                 entity.Property(f => f.CreatedAt)
//                       .HasDefaultValueSql("CURRENT_TIMESTAMP");
//                 entity.Property(f => f.UpdatedAt)
//                       .HasDefaultValueSql("CURRENT_TIMESTAMP");
//                 entity.Property(f => f.IsFolder).HasDefaultValueSql("0");
//                 entity.Property(f => f.HasContent).HasDefaultValueSql("0");

//                 entity.HasOne(f => f.Owner)
//                       .WithMany(u => u.FileItems)
//                       .HasForeignKey(f => f.OwnerID)
//                       .OnDelete(DeleteBehavior.Cascade);
//             });

//             modelBuilder.Entity<SharedFileItem>(entity =>
//             {
//                 entity.HasKey(fs => new { fs.FileID, fs.UserID });

//                 entity.HasOne(fs => fs.File)
//                       .WithMany(f => f.SharedWith)
//                       .HasForeignKey(fs => fs.FileID)
//                       .OnDelete(DeleteBehavior.Cascade);

//                 entity.HasOne(fs => fs.User)
//                       .WithMany(u => u.SharedFileItems)
//                       .HasForeignKey(fs => fs.UserID)
//                       .OnDelete(DeleteBehavior.Cascade);

//                 entity.Property(fs => fs.Permission)
//                       .HasConversion<string>()
//                       .HasMaxLength(10);
//             });
//             modelBuilder.Entity<Session>(entity =>
//             {
//                 entity.HasKey(s => s.ID);

//                 entity.Property(s => s.Token)
//                     .IsRequired()
//                     .HasMaxLength(255);

//                 entity.Property(s => s.UserID)
//                     .IsRequired();

//                 entity.Property(s => s.CreatedAt)
//                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

//                 entity.Property(s => s.ExpiresAt);

//                 entity.HasOne(s => s.User)
//                     .WithOne(u => u.Session)
//                     .HasForeignKey<Session>(s => s.UserID)
//                     .OnDelete(DeleteBehavior.Cascade);
//             });

//         }
//     }
// }