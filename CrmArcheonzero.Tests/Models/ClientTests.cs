using System;
using Xunit;
using FluentAssertions;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Tests.Models
{
    public class ClientTests
    {
        [Fact]
        public void Client_ShouldSetPropertiesCorrectly()
        {
            var now = DateTime.Now;
            var client = new Client
            {
                Id = 1,
                Name = "Тестовый клиент",
                Phone = "+7 (999) 123-45-67",
                Email = "test@example.com",
                Status = "Active",
                CreatedAt = now,
                Birthday = new DateTime(1990, 1, 1),
                Company = "ООО Тест",
                Position = "Менеджер",
                Address = "ул. Тестовая, 1",
                Source = "Website",
                Tags = "важный,новый",
                AssignedUserId = 1
            };

            client.Id.Should().Be(1);
            client.Name.Should().Be("Тестовый клиент");
            client.Email.Should().Be("test@example.com");
            client.Status.Should().Be("Active");
            client.CreatedAt.Should().Be(now);
            client.Birthday.Should().Be(new DateTime(1990, 1, 1));
            client.Company.Should().Be("ООО Тест");
            client.AssignedUserId.Should().Be(1);
        }

        [Fact]
        public void Client_Collections_ShouldBeInitialized()
        {
            var client = new Client();
            client.Interactions.Should().NotBeNull();
            client.Tasks.Should().NotBeNull();
            client.ClientNotes.Should().NotBeNull();
        }
    }
}