using Magicodes.ExporterAndImporter.Core;
using System;

namespace CrmArcheonzero.DTO
{
    public class ClientImportDto
    {
        [ImporterHeader(Name = "Имя")]
        public string? Name { get; set; }

        [ImporterHeader(Name = "Телефон")]
        public string? Phone { get; set; }

        [ImporterHeader(Name = "Email")]
        public string? Email { get; set; }

        [ImporterHeader(Name = "Компания")]
        public string? Company { get; set; }

        [ImporterHeader(Name = "Статус")]
        public string? Status { get; set; }

        // ===== НОВЫЕ ПОЛЯ =====
        [ImporterHeader(Name = "Источник")]
        public string? Source { get; set; }

        [ImporterHeader(Name = "Теги")]
        public string? Tags { get; set; }

        [ImporterHeader(Name = "Дата рождения")]
        public DateTime? Birthday { get; set; }

        [ImporterHeader(Name = "Примечание")]
        public string? Notes { get; set; }

        [ImporterHeader(Name = "Ответственный")]
        public string? AssignedUser { get; set; }
    }
}