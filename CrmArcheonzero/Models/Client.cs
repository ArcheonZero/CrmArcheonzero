using System;
using System.Collections.Generic;

namespace CrmArcheonzero.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; } // Active, Inactive, Lead
        public string? Notes { get; set; }
        public DateTime? LastContact { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? Address { get; set; }
        public string? Source { get; set; }
        public string? Tags { get; set; }
        public int? AssignedUserId { get; set; }

        // ===== НОВЫЕ ПОЛЯ ДЛЯ КОРЗИНЫ =====
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Навигационные свойства
        public User? AssignedUser { get; set; }
        public List<Interaction> Interactions { get; set; } = new();
        public List<ClientTask> Tasks { get; set; } = new();
        public List<Note> ClientNotes { get; set; } = new();
    }
}