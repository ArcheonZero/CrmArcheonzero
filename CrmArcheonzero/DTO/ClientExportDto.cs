using Magicodes.ExporterAndImporter.Core;
using System;

namespace CrmArcheonzero.DTO
{
    public class ClientExportDto
    {
        [ExporterHeader(DisplayName = "ID")]
        public int Id { get; set; }

        [ExporterHeader(DisplayName = "Имя")]
        public string? Name { get; set; }

        [ExporterHeader(DisplayName = "Телефон")]
        public string? Phone { get; set; }

        [ExporterHeader(DisplayName = "Email")]
        public string? Email { get; set; }

        [ExporterHeader(DisplayName = "Компания")]
        public string? Company { get; set; }

        [ExporterHeader(DisplayName = "Статус")]
        public string? Status { get; set; }

        [ExporterHeader(DisplayName = "Источник")]
        public string? Source { get; set; }

        [ExporterHeader(DisplayName = "Теги")]
        public string? Tags { get; set; }

        [ExporterHeader(DisplayName = "Дата рождения")]
        public DateTime? Birthday { get; set; }

        [ExporterHeader(DisplayName = "Ответственный")]
        public string? AssignedUser { get; set; }

        [ExporterHeader(DisplayName = "Примечание")]
        public string? Notes { get; set; }
    }
}