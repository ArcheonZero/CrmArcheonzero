using Microsoft.EntityFrameworkCore;
using CrmArcheonzero.Models;
using System.Linq;
using System;

namespace CrmArcheonzero.Data
{
    public class SqlServerDbContext : DbContext, IDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ClientTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; } // НОВОЕ

        private readonly string _connectionString;

        public SqlServerDbContext(string connectionString = "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;")
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === СУЩЕСТВУЮЩИЕ НАСТРОЙКИ ===
            modelBuilder.Entity<Client>()
                .Property(c => c.Status)
                .HasDefaultValue("Lead");

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Interactions)
                .WithOne(i => i.Client)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Client)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.ClientNotes)
                .WithOne(n => n.Client)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("IX_Clients_Name");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .HasDatabaseName("IX_Clients_Email");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Status)
                .HasDatabaseName("IX_Clients_Status");

            // === НОВЫЕ НАСТРОЙКИ ДЛЯ AssignmentHistory ===
            modelBuilder.Entity<AssignmentHistory>()
                .HasOne(ah => ah.Client)
                .WithMany()
                .HasForeignKey(ah => ah.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentHistory>()
                .HasOne(ah => ah.FromUser)
                .WithMany()
                .HasForeignKey(ah => ah.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssignmentHistory>()
                .HasOne(ah => ah.ToUser)
                .WithMany()
                .HasForeignKey(ah => ah.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssignmentHistory>()
                .HasOne(ah => ah.AssignedByUser)
                .WithMany()
                .HasForeignKey(ah => ah.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Message).IsRequired();
                entity.HasOne(m => m.User)
                      .WithMany()
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public void EnsureDatabaseCreated()
        {
            Database.EnsureCreated();
        }

        public void EnsureSeedData()
        {
            if (Users.Any()) return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@crm.com",
                FullName = "Администратор",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            Users.Add(admin);
            SaveChanges();
        }
    }
}