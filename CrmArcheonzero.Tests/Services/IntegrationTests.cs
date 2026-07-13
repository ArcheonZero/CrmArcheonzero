using CrmArcheonzero.Data;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;
using CrmArcheonzero.Tests.Helpers;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace CrmArcheonzero.Tests.Services
{
    public class IntegrationTests : IDisposable
    {
        private readonly InMemoryDbContext _context;
        private readonly ClientRepository _repository;
        private readonly AuthService _authService;

        public IntegrationTests()
        {
            _context = new InMemoryDbContext(Guid.NewGuid().ToString());
            _repository = new ClientRepository(_context);
            _authService = new AuthService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void FullCycle_AddClient_UpdateClient_DeleteClient_Search_ShouldWork()
        {
            var user = TestDataHelper.GetTestUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var client = new Client
            {
                Name = "Тестовый клиент",
                Phone = "+7 (999) 111-22-33",
                Email = "test@integration.com",
                Status = "Lead",
                CreatedAt = DateTime.Now,
                AssignedUserId = user.Id,
                Position = "Менеджер",
                Company = "ООО Интеграция",
                Address = "ул. Интеграционная, 1",
                Source = "Test",
                Tags = "интеграция"
            };
            _repository.Add(client);
            var saved = _repository.GetById(client.Id);
            saved.Should().NotBeNull();
            saved.Name.Should().Be("Тестовый клиент");

            saved.Name = "Тестовый клиент (обновлён)";
            _repository.Update(saved);
            var updated = _repository.GetById(saved.Id);
            updated.Name.Should().Be("Тестовый клиент (обновлён)");

            var searchResult = _repository.Search("обновлён");
            searchResult.Should().HaveCount(1);
            searchResult[0].Name.Should().Be("Тестовый клиент (обновлён)");

            // === ИСПРАВЛЕНО ===
            _repository.PermanentDelete(saved.Id);   // вместо _repository.Delete(saved.Id)

            var deleted = _repository.GetById(saved.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public void FullCycle_AuthAndClient_ShouldWorkTogether()
        {
            var user = new User
            {
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
                IsActive = true,
                Role = "User",
                CreatedAt = DateTime.Now,
                Email = "test@test.com",
                FullName = "Test User",
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new()
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var loginResult = _authService.Login("testuser", "pass123");
            loginResult.Should().BeTrue();
            _authService.GetCurrentUser().Username.Should().Be("testuser");

            var client = new Client
            {
                Name = "Клиент интеграции",
                Phone = "+7 (999) 888-77-66",
                Email = "integration@test.com",
                Status = "Active",
                CreatedAt = DateTime.Now,
                AssignedUserId = _authService.GetCurrentUser().Id,
                Position = "Интегратор",
                Company = "ООО Интеграция",
                Address = "ул. Интеграционная, 2",
                Source = "Integration Test",
                Tags = "важный"
            };
            _repository.Add(client);

            var saved = _repository.GetById(client.Id);
            saved.Should().NotBeNull();
            saved.AssignedUserId.Should().Be(_authService.GetCurrentUser().Id);

            _authService.Logout();
            _authService.GetCurrentUser().Should().BeNull();
        }
    }
}