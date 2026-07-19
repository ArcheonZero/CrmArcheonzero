using System;
using System.Collections.Generic;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static List<Client> GetTestClients()
        {
            return new List<Client>
            {
                new Client
                {
                    Id = 1,
                    Name = "Иван Петров",
                    Phone = "+7 (912) 345-67-89",
                    Email = "ivan@mail.ru",
                    Status = "Active",
                    Company = "ООО ТехноСервис",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    Birthday = new DateTime(1985, 5, 15),
                    Position = "Менеджер",
                    Address = "ул. Тестовая, 1",
                    Source = "Website",
                    Tags = "важный",
                    AssignedUserId = 1
                },
                new Client
                {
                    Id = 2,
                    Name = "Мария Сидорова",
                    Phone = "+7 (903) 222-33-44",
                    Email = "maria@yandex.ru",
                    Status = "Lead",
                    Company = "ИП Сидорова",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Position = "Директор",
                    Address = "ул. Центральная, 5",
                    Source = "Referral",
                    Tags = "новый",
                    AssignedUserId = 2
                },
                new Client
                {
                    Id = 3,
                    Name = "Алексей Иванов",
                    Phone = "+7 (911) 555-66-77",
                    Email = "alex@google.com",
                    Status = "Inactive",
                    Company = "ООО Альфа",
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    Position = "Разработчик",
                    Address = "ул. Программистов, 10",
                    Source = "Social",
                    Tags = "старый",
                    AssignedUserId = 3
                }
            };
        }

        public static User GetTestUser()
        {
            return new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Email = "test@test.com",
                FullName = "Test User",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new List<Client>()
            };
        }

        public static User GetTestAdmin()
        {
            return new User
            {
                Id = 2,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@crm.com",
                FullName = "Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "",
                RefreshTokenExpiry = null,
                AssignedClients = new List<Client>()
            };
        }

        public static Interaction GetTestInteraction()
        {
            return new Interaction
            {
                Id = 1,
                ClientId = 1,
                Date = DateTime.UtcNow.AddDays(-5),
                Type = "Call",
                Description = "Тестовый звонок",
                Outcome = "Успешно"
            };
        }

        public static ClientTask GetTestClientTask()
        {
            return new ClientTask
            {
                Id = 1,
                ClientId = 1,
                Title = "Тестовая задача",
                Description = "Описание задачи",
                DueDate = DateTime.UtcNow.AddDays(7),
                Priority = "High",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Note GetTestNote()
        {
            return new Note
            {
                Id = 1,
                ClientId = 1,
                Content = "Тестовая заметка",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}