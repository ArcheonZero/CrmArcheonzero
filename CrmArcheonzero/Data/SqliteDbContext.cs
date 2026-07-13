using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CrmArcheonzero.Models;
using BCrypt.Net;

namespace CrmArcheonzero.Data
{
    public class SqliteDbContext : DbContext, IDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<ClientTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; } // НОВОЕ

        private readonly string _connectionString;

        public SqliteDbContext(string connectionString = "Data Source=crm.db")
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === НАСТРОЙКА ДЛЯ USER ===
            modelBuilder.Entity<User>(entity =>
            {
                // Имя пользователя — уникальное
                entity.HasIndex(u => u.Username).IsUnique();

                // Роль — обязательная, по умолчанию "User"
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("User");
            });

            // === НАСТРОЙКА ДЛЯ CLIENT ===
            modelBuilder.Entity<Client>()
                .Property(c => c.Status)
                .HasDefaultValue("Lead");
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

            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedClients)
                .WithOne(c => c.AssignedUser)
                .HasForeignKey(c => c.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("IX_Clients_Name");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .HasDatabaseName("IX_Clients_Email");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Phone)
                .HasDatabaseName("IX_Clients_Phone");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Status)
                .HasDatabaseName("IX_Clients_Status");

            modelBuilder.Entity<ClientTask>()
                .HasIndex(t => t.DueDate)
                .HasDatabaseName("IX_Tasks_DueDate");

            modelBuilder.Entity<ClientTask>()
                .HasIndex(t => t.IsCompleted)
                .HasDatabaseName("IX_Tasks_IsCompleted");

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
            // === ДОБАВЛЯЕМ ТЕСТОВЫХ ПОЛЬЗОВАТЕЛЕЙ ===
            var manager = new User
            {
                Username = "manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                Email = "manager@crm.com",
                FullName = "Менеджер",
                Role = "Manager",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            Users.Add(manager);

            var super = new User
            {
                Username = "super",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                Email = "super@crm.com",
                FullName = "Super менеджер",
                Role = "SuperManager",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            Users.Add(super);

            var user = new User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Email = "user@crm.com",
                FullName = "Пользователь",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            Users.Add(user);

            SaveChanges();

            if (Clients.Any()) return;

            var clients = new List<Client>
            {
                new Client
                {
                    Name = "Иван Петров",
                    Phone = "+7 (912) 345-67-89",
                    Email = "ivan@mail.ru",
                    Status = "Active",
                    Company = "ООО ТехноСервис",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    Birthday = new DateTime(1985, 5, 15)
                },
                new Client
                {
                    Name = "Мария Сидорова",
                    Phone = "+7 (903) 222-33-44",
                    Email = "maria@yandex.ru",
                    Status = "Lead",
                    Company = "ИП Сидорова",
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new Client
                {
                    Name = "Алексей Иванов",
                    Phone = "+7 (911) 555-66-77",
                    Email = "alex@google.com",
                    Status = "Inactive",
                    Company = "ООО Альфа",
                    CreatedAt = DateTime.Now.AddDays(-60)
                }
            };

            Clients.AddRange(clients);
            SaveChanges();
        }
    }
}