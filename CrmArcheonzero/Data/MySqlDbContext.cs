using CrmArcheonzero.Models;
using CrmArcheonzero.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CrmArcheonzero.Data
{
    public class MySqlDbContext : DbContext, IDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<ClientTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; }

        private readonly string _connectionString;

        public MySqlDbContext(string connectionString)
        {
            _connectionString = connectionString;
            LoggerService.LogAction("MySqlDbContext", $"Создан контекст со строкой: {_connectionString}");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_connectionString,
                new MySqlServerVersion(new Version(8, 0, 35)),
                options => options.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === ВСЕ НАСТРОЙКИ ТОЧНО ТАКИЕ ЖЕ, КАК В SqliteDbContext ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("User");
            });

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
            try
            {
                LoggerService.LogAction("MySqlDbContext", $"Попытка подключения к MySQL...");
                LoggerService.LogAction("MySqlDbContext", $"Строка подключения: {_connectionString}");

                // Проверяем, что соединение вообще возможно
                var canConnect = Database.CanConnect();
                LoggerService.LogAction("MySqlDbContext", $"CanConnect вернул: {canConnect}");

                // Если CanConnect не сработал, пробуем открыть соединение вручную
                try
                {
                    Database.OpenConnection();
                    LoggerService.LogAction("MySqlDbContext", "OpenConnection успешно открыл соединение.");
                    Database.CloseConnection();
                    LoggerService.LogAction("MySqlDbContext", "CloseConnection закрыл соединение.");
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "MySqlDbContext.OpenConnection");
                    throw;
                }

                // Проверяем, что база существует
                var dbExists = Database.CanConnect();
                LoggerService.LogAction("MySqlDbContext", $"База существует: {dbExists}");
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "MySqlDbContext.EnsureDatabaseCreated");
                throw;
            }
        }

        public void EnsureSeedData()
        {
            try
            {
                // Проверяем, есть ли таблица Users
                var tableExists = Database.ExecuteSqlRaw(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Users';"
                ) > 0;

                if (!tableExists)
                {
                    // Если таблиц нет — создаём через EnsureCreated
                    Database.EnsureCreated();
                }
                // Проверяем, есть ли пользователи
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
                LoggerService.LogError(ex, "MySqlDbContext.EnsureSeedData");
                throw;
            }
        }
    }
}