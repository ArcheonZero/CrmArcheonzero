using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly InMemoryDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _context = new InMemoryDbContext(Guid.NewGuid().ToString());
            _authService = new AuthService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // ... существующие тесты (Login, IsAdmin, CreateUser, ChangePassword, Logout) без изменений ...

        // ===== НОВЫЙ ТЕСТ ДЛЯ SUPERMANAGER =====

        [Fact]
        public void IsSuperManager_UserIsSuperManager_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "supermanager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
                IsActive = true,
                Role = "SuperManager",
                CreatedAt = DateTime.Now,
                Email = "super@test.com",
                FullName = "Super Manager",
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new()
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            _authService.Login("supermanager", "pass123");

            // Act
            var result = _authService.IsSuperManager();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsSuperManager_UserIsAdmin_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsActive = true,
                Role = "Admin",
                CreatedAt = DateTime.Now,
                Email = "admin@test.com",
                FullName = "Admin User",
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new()
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            _authService.Login("admin", "admin123");

            // Act
            var result = _authService.IsSuperManager();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsSuperManager_UserIsManager_ShouldReturnFalse()
        {
            // Arrange
            var user = new User
            {
                Username = "manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
                IsActive = true,
                Role = "Manager",
                CreatedAt = DateTime.Now,
                Email = "manager@test.com",
                FullName = "Manager User",
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new()
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            _authService.Login("manager", "pass123");

            // Act
            var result = _authService.IsSuperManager();

            // Assert
            result.Should().BeFalse();
        }
    }
}