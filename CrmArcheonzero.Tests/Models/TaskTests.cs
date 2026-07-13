using System;
using Xunit;
using FluentAssertions;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Tests.Models
{
    public class TaskTests
    {
        [Fact]
        public void ClientTask_ShouldSetPropertiesCorrectly()
        {
            var now = DateTime.Now;
            var task = new ClientTask
            {
                Id = 1,
                ClientId = 1,
                Title = "Тестовая задача",
                Description = "Описание",
                DueDate = now.AddDays(7),
                Priority = "High",
                IsCompleted = false,
                CreatedAt = now
            };

            task.Id.Should().Be(1);
            task.ClientId.Should().Be(1);
            task.Title.Should().Be("Тестовая задача");
            task.Priority.Should().Be("High");
            task.IsCompleted.Should().BeFalse();
        }
    }
}