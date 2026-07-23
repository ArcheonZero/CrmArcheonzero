using CrmArcheonzero.Models;
using CrmArcheonzero.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace CrmArcheonzero.Data
{
    public class PostgreDbContext : DbContext, IDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<ClientTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; }

        private readonly string _connectionString;

        public PostgreDbContext(string connectionString)
        {
            _connectionString = connectionString;
            LoggerService.LogAction("PostgreDbContext", $"Создан контекст с строкой: {_connectionString}");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString, options =>
                {
                    options.EnableRetryOnFailure();
                });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === НАСТРОЙКА ДЛЯ USER ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("User");
            });

            // === НАСТРОЙКА ДЛЯ CLIENT ===
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
                .HasMany(u => u.AssignedClients)
                .WithOne(c => c.AssignedUser)
                .HasForeignKey(c => c.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // === ИНДЕКСЫ ===
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

            // === НАСТРОЙКИ ДЛЯ AssignmentHistory ===
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

            // === НАСТРОЙКИ ДЛЯ ЧАТА ===
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
            try
            {
                Database.OpenConnection();
                Database.CloseConnection();
                LoggerService.LogAction("PostgreDbContext", "Подключение к PostgreSQL установлено.");
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "PostgreDbContext.EnsureDatabaseCreated");
                MessageBox.Show(
                    "Не удалось подключиться к базе данных PostgreSQL.\n" +
                    "Проверьте настройки подключения или обратитесь к администратору.\n" +
                    $"Ошибка: {ex.Message}",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                throw;
            }
        }

        public void EnsureSeedData()
        {
            try
            {
                // Проверяем, есть ли таблица Users
                var tableExists = Database.ExecuteSqlRaw(
                    "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Users');"
                ) > 0;

                if (!tableExists)
                {
                    // Если таблиц нет — создаём через EnsureCreated
                    Database.EnsureCreated();
                }

                // Если пользователи уже есть — ничего не делаем
                if (Users.Any()) return;

                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@crm.com",
                    FullName = "Администратор",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                Users.Add(admin);

                var manager = new User
                {
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    Email = "manager@crm.com",
                    FullName = "Менеджер",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow
                };
                Users.Add(user);

                SaveChanges();

                // Если клиенты уже есть — ничего не делаем
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
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Birthday = new DateTime(1985, 5, 15),
                AssignedUserId = admin.Id
            },
            new Client
            {
                Name = "Мария Сидорова",
                Phone = "+7 (903) 222-33-44",
                Email = "maria@yandex.ru",
                Status = "Lead",
                Company = "ИП Сидорова",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                AssignedUserId = manager.Id
            },
            new Client
            {
                Name = "Алексей Иванов",
                Phone = "+7 (911) 555-66-77",
                Email = "alex@google.com",
                Status = "Inactive",
                Company = "ООО Альфа",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                AssignedUserId = super.Id
            }
        };

                Clients.AddRange(clients);
                SaveChanges();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "PostgreDbContext.EnsureSeedData");
                throw;
            }
        }
    }
}