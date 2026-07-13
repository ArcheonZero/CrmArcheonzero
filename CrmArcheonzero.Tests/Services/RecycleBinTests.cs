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
    public class RecycleBinTests : IDisposable
    {
        private readonly InMemoryDbContext _context;
        private readonly ClientRepository _repository;

        public RecycleBinTests()
        {
            _context = new InMemoryDbContext(Guid.NewGuid().ToString());
            _repository = new ClientRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndDeletedBy()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();
            var userId = 5;

            // Act
            _repository.SoftDelete(client.Id, userId);
            var result = _repository.GetById(client.Id, true);

            // Assert
            result.DeletedAt.Should().NotBeNull();
            result.DeletedByUserId.Should().Be(userId);
        }

        [Fact]
        public void Restore_ShouldClearDeletedFields()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();
            _repository.SoftDelete(client.Id, 1);

            // Act
            _repository.Restore(client.Id);
            var result = _repository.GetById(client.Id, true);

            // Assert
            result.DeletedAt.Should().BeNull();
            result.DeletedByUserId.Should().BeNull();
        }

        [Fact]
        public void PermanentDelete_ShouldRemoveAllClientData()
        {
            // Arrange
            var client = TestDataHelper.GetTestClients()[0];
            _context.Clients.Add(client);
            _context.SaveChanges();
            _repository.SoftDelete(client.Id, 1);

            // Act
            _repository.PermanentDelete(client.Id);

            // Assert
            var allClients = _repository.GetAll(true);
            allClients.Should().NotContain(c => c.Id == client.Id);
        }
    }
}