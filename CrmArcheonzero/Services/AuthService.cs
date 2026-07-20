using BCrypt.Net;
using CrmArcheonzero.Data;
using CrmArcheonzero.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace CrmArcheonzero.Services
{
    public class AuthService
    {
        private IDbContext? _context;
        private User? _currentUser;

        public AuthService()
        {
            // Контекст не создаётся здесь
        }

        public AuthService(IDbContext context)
        {
            _context = context;
        }

        public bool Login(string username, string password, string provider = "Sqlite")
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                var connectionString = GetConnectionString(provider);
                DbContextFactory.SetProvider(provider, connectionString);
                _context = DbContextFactory.GetDbContext();

                var user = ((DbContext)_context).Set<User>()
                    .FirstOrDefault(u => u.Username == username && u.IsActive);

                if (user == null)
                {
                    LoggerService.LogAction("Ошибка входа", $"Пользователь {username} не найден или неактивен (БД: {provider})");
                    return false;
                }

                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _currentUser = user;
                    user.LastLogin = DateTime.UtcNow;
                    _context.SaveChanges();

                    LoggerService.LogAction("Вход", $"Пользователь {user.Username} вошёл в систему (БД: {provider})");
                    MessageBox.Show($"Установлен пользователь: {_currentUser?.Username}, роль: {_currentUser?.Role}");
                    return true;
                }
                else
                {
                    LoggerService.LogAction("Ошибка входа", $"Неверный пароль для пользователя {username} (БД: {provider})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, $"AuthService.Login (БД: {provider})");
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public string GetConnectionString(string provider)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return provider.ToLower() switch
            {
                "postgre" or "postgresql" or "npgsql" => config["Database:Providers:PostgreSQL:ConnectionString"] ?? "Host=localhost;Database=crmdb;Username=postgres;Password=postgres",
                "sqlserver" => config["Database:Providers:SqlServer:ConnectionString"] ?? "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;",
                _ => config["Database:Providers:Sqlite:ConnectionString"] ?? "Data Source=crm.db"
            };
        }

        public bool Login(string username, string password)
        {
            return Login(username, password, "Sqlite");
        }

        public void Logout()
        {
            if (_currentUser != null)
            {
                LoggerService.LogAction("Выход", $"Пользователь {_currentUser.Username} вышел из системы");
            }
            _currentUser = null;
            DbContextFactory.ResetDbContext();
        }

        public User? GetCurrentUser() => _currentUser;

        public bool IsAdmin() => _currentUser?.Role == "Admin";

        public bool IsSuperManager() => _currentUser?.Role == "SuperManager" || _currentUser?.Role == "Admin";

        public bool IsAuthenticated() => _currentUser != null;

        public bool HasPermission(string requiredRole)
        {
            if (_currentUser == null) return false;
            if (_currentUser.Role == "Admin") return true;
            return _currentUser.Role == requiredRole;
        }

        public bool CreateUser(string username, string password, string email, string fullName, string role = "User")
        {
            if (_context == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var dbContext = (DbContext)_context;
                if (dbContext.Set<User>().Any(u => u.Username == username))
                    return false;

                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Set<User>().Add(user);
                _context.SaveChanges();

                LoggerService.LogAction("Создание пользователя", $"Пользователь {username} создан с ролью {role}");
                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AuthService.CreateUser");
                MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (_context == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var user = ((DbContext)_context).Set<User>().Find(userId);
                if (user == null) return false;
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                    return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.SaveChanges();

                LoggerService.LogAction("Смена пароля", $"Пользователь {user.Username} сменил пароль");
                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AuthService.ChangePassword");
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool UpdateProfile(int userId, string newFullName, string newEmail)
        {
            if (_context == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var user = ((DbContext)_context).Set<User>().Find(userId);
                if (user == null) return false;

                user.FullName = newFullName;
                user.Email = newEmail;
                _context.SaveChanges();

                LoggerService.LogAction("Обновление профиля", $"Пользователь {user.Username} обновил имя/email");
                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AuthService.UpdateProfile");
                MessageBox.Show($"Ошибка при обновлении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public string? GenerateResetToken(string email)
        {
            if (_context == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                var user = ((DbContext)_context).Set<User>().FirstOrDefault(u => u.Email == email);
                if (user == null) return null;

                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                user.RefreshToken = token;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(24);
                _context.SaveChanges();
                return token;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AuthService.GenerateResetToken");
                return null;
            }
        }

        public bool ResetPassword(string token, string newPassword)
        {
            if (_context == null)
            {
                MessageBox.Show("Сначала подключитесь к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var user = ((DbContext)_context).Set<User>()
                    .FirstOrDefault(u => u.RefreshToken == token && u.RefreshTokenExpiry > DateTime.UtcNow);

                if (user == null) return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                _context.SaveChanges();

                LoggerService.LogAction("Сброс пароля", $"Пароль сброшен для пользователя {user.Username}");
                return true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AuthService.ResetPassword");
                MessageBox.Show($"Ошибка при сбросе пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}