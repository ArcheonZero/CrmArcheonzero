using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;
using CrmArcheonzero.Services;
using CrmArcheonzero.Tests.Helpers;

namespace CrmArcheonzero.Tests.Services
{
    public class ClientRepositoryTests : IDisposable
    {
        private readonly InMemoryDbContext _context;
        private readonly ClientRepository _repository;

        public ClientRepositoryTests()
        {
            _context = new InMemoryDbContext(Guid.NewGuid().ToString());
            _repository = new ClientRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // ===== СУЩЕСТВУЮЩИЕ ТЕСТЫ (без изменений) =====

        [Fact]
        public void GetAll_ShouldReturnAllClients()
        {
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var result = _repository.GetAll();

            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Name == "Иван Петров");
            result.Should().Contain(c => c.Name == "Мария Сидорова");
        }

        [Fact]
        public void GetById_ExistingClient_ShouldReturnClient()
        {
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            var result = _repository.GetById(client.Id);

            result.Should().NotBeNull();
            result.Name.Should().Be("Иван Петров");
        }

        [Fact]
        public void GetById_NonExistingClient_ShouldReturnNull()
        {
            var result = _repository.GetById(999);
            result.Should().BeNull();
        }

        [Fact]
        public void Add_ShouldAddClient()
        {
            var client = new Client
            {
                Name = "Новый клиент",
                Phone = "+7 (999) 111-22-33",
                Email = "new@client.com",
                Status = "Lead",
                CreatedAt = DateTime.UtcNow,
                Position = "Менеджер",
                Address = "ул. Новая, 1",
                Source = "Website",
                Tags = "новый",
                AssignedUserId = 1
            };

            _repository.Add(client);

            var saved = _context.Clients.FirstOrDefault(c => c.Name == "Новый клиент");
            saved.Should().NotBeNull();
            saved.Email.Should().Be("new@client.com");
            saved.Status.Should().Be("Lead");
        }

        [Fact]
        public void Update_ShouldModifyClient()
        {
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            client.Name = "Иван Петров (обновлён)";
            _repository.Update(client);

            var updated = _context.Clients.Find(client.Id);
            updated.Name.Should().Be("Иван Петров (обновлён)");
        }

        [Fact]
        public void Search_ByQuery_ShouldReturnMatchingClients()
        {
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var results = _repository.Search("Иван");

            results.Should().HaveCountGreaterThan(0);
            results.All(c => c.Name.Contains("Иван")).Should().BeTrue();
            results.Should().Contain(c => c.Name == "Иван Петров");
        }

        [Fact]
        public void Search_EmptyQuery_ShouldReturnAll()
        {
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var results = _repository.Search("");

            results.Should().HaveCount(3);
        }

        [Fact]
        public void GetStatistics_ShouldReturnCorrectCounts()
        {
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var stats = _repository.GetStatistics();

            stats["Total"].Should().Be(3);
            stats["Active"].Should().Be(1);
            stats["Inactive"].Should().Be(1);
            stats["Lead"].Should().Be(1);
        }

        [Fact]
        public void ClientExists_ExistingEmail_ShouldReturnTrue()
        {
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            var exists = _repository.ClientExists("ivan@mail.ru");

            exists.Should().BeTrue();
        }

        [Fact]
        public void ClientExists_NonExistingEmail_ShouldReturnFalse()
        {
            var exists = _repository.ClientExists("nonexistent@mail.ru");
            exists.Should().BeFalse();
        }

        [Fact]
        public void GetClientsWithBirthdayInMonth_ShouldReturnCorrectClients()
        {
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var result = _repository.GetClientsWithBirthdayInMonth(5);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Иван Петров");
        }

        [Fact]
        public void DetachClient_ShouldDetachEntity()
        {
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            _repository.DetachClient(client);

            var entry = _context.Entry(client);
            entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
        }

        // ============================================================
        // НОВЫЕ ТЕСТЫ ДЛЯ КОРЗИНЫ
        // ============================================================

        [Fact]
        public void SoftDelete_ShouldMarkClientAsDeleted()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();
            var userId = 1;

            // Act
            _repository.SoftDelete(client.Id, userId);
            var deleted = _repository.GetById(client.Id, true);

            // Assert
            deleted.Should().NotBeNull();
            deleted.IsDeleted.Should().BeTrue();
            deleted.DeletedAt.Should().NotBeNull();
            deleted.DeletedByUserId.Should().Be(userId);
        }

        [Fact]
        public void GetAll_ShouldNotReturnDeletedClients()
        {
            // Arrange
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var userId = 1;
            var clientToDelete = clients[0];
            _repository.SoftDelete(clientToDelete.Id, userId);

            // Act
            var result = _repository.GetAll(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(c => c.Id == clientToDelete.Id);
        }

        [Fact]
        public void GetAll_IncludeDeleted_ShouldReturnAllClients()
        {
            // Arrange
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var userId = 1;
            var clientToDelete = clients[0];
            _repository.SoftDelete(clientToDelete.Id, userId);

            // Act
            var result = _repository.GetAll(true);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Id == clientToDelete.Id);
        }

        [Fact]
        public void Restore_ShouldRemoveDeletedFlag()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            var userId = 1;
            _repository.SoftDelete(client.Id, userId);

            // Act
            _repository.Restore(client.Id);
            var restored = _repository.GetById(client.Id, true);

            // Assert
            restored.Should().NotBeNull();
            restored.IsDeleted.Should().BeFalse();
            restored.DeletedAt.Should().BeNull();
            restored.DeletedByUserId.Should().BeNull();
        }

        [Fact]
        public void PermanentDelete_ShouldRemoveClientCompletely()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();

            var userId = 1;
            _repository.SoftDelete(client.Id, userId);

            // Act
            _repository.PermanentDelete(client.Id);
            var deleted = _repository.GetById(client.Id, true);

            // Assert
            deleted.Should().BeNull();
        }

        [Fact]
        public void GetDeleted_ShouldReturnOnlyDeletedClients()
        {
            // Arrange
            var clients = TestDataHelper.GetTestClients();
            _context.Clients.AddRange(clients);
            _context.SaveChanges();

            var userId = 1;
            var clientToDelete = clients[0];
            _repository.SoftDelete(clientToDelete.Id, userId);

            // Act
            var result = _repository.GetDeleted();

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(clientToDelete.Id);
            result[0].IsDeleted.Should().BeTrue();
        }
    }
}