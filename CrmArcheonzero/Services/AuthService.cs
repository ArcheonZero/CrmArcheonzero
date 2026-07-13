using BCrypt.Net;
using CrmArcheonzero.Data;
using CrmArcheonzero.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace CrmArcheonzero.Services
{
    public class AuthService
    {
        private readonly IDbContext _context;
        private User? _currentUser;

        public AuthService()
        {
            _context = DbContextFactory.GetDbContext();
        }

        public AuthService(IDbContext context)
        {
            _context = context;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var user = ((DbContext)_context).Set<User>()
                .FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null) return false;

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _currentUser = user;
                user.LastLogin = DateTime.Now;
                _context.SaveChanges();
                
                LoggerService.LogAction("Вход", $"Пользователь {user.Username} вошёл в систему");
                MessageBox.Show($"Установлен пользователь: {_currentUser?.Username}, роль: {_currentUser?.Role}");
                return true;
            }

            return false;
        }

        public void Logout()
        {
            if (_currentUser != null)
            {
                LoggerService.LogAction("Выход", $"Пользователь {_currentUser.Username} вышел из системы");
            }
            _currentUser = null;
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
                CreatedAt = DateTime.Now
            };

            dbContext.Set<User>().Add(user);
            _context.SaveChanges();
            
            LoggerService.LogAction("Создание пользователя", $"Пользователь {username} создан с ролью {role}");
            return true;
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
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

        public bool UpdateProfile(int userId, string newFullName, string newEmail)
        {
            var user = ((DbContext)_context).Set<User>().Find(userId);
            if (user == null) return false;

            user.FullName = newFullName;
            user.Email = newEmail;
            _context.SaveChanges();

            LoggerService.LogAction("Обновление профиля", $"Пользователь {user.Username} обновил имя/email");
            return true;
        }
        public string? GenerateResetToken(string email)
        {
            var user = ((DbContext)_context).Set<User>().FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.RefreshToken = token;
            user.RefreshTokenExpiry = DateTime.Now.AddHours(24);
            _context.SaveChanges();
            return token;
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var user = ((DbContext)_context).Set<User>()
                .FirstOrDefault(u => u.RefreshToken == token && u.RefreshTokenExpiry > DateTime.Now);

            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            _context.SaveChanges();
            
            LoggerService.LogAction("Сброс пароля", $"Пароль сброшен для пользователя {user.Username}");
            return true;
        }
    }
}