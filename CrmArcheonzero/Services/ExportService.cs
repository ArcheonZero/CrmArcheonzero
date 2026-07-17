using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Csv;
using Magicodes.ExporterAndImporter.Pdf;
using Magicodes.ExporterAndImporter.Word;
using CrmArcheonzero.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CrmArcheonzero.Services
{
    public class ExportService
    {
        // ============================================================
        // ОСНОВНОЙ МЕТОД ЭКСПОРТА
        // ============================================================
        public byte[] ExportClients(List<ClientExportDto> data, string format, string? templatePath = null)
        {
            return format.ToLower() switch
            {
                "xlsx" => new ExcelExporter().ExportAsByteArray(data).GetAwaiter().GetResult(),
                "csv" => new CsvExporter().ExportAsByteArray(data).GetAwaiter().GetResult(),
                "html" => ExportToHtml(data),
                _ => throw new NotSupportedException($"Формат {format} не поддерживается")
            };
        }

        // ============================================================
        // PDF (шаблон — путь к файлу)
        // ============================================================
        private byte[] ExportPdf(List<ClientExportDto> data, string? templatePath)
        {
            if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
                throw new FileNotFoundException("Файл шаблона для PDF не найден.", templatePath);

            var templateContent = File.ReadAllText(templatePath);
            var tempFile = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.pdf");
            var exporter = new PdfExporter();

            // Берём первого клиента из списка
            var singleData = data.FirstOrDefault();
            if (singleData == null)
                throw new InvalidOperationException("Нет данных для экспорта");

            // ✅ ExportByTemplate для одного объекта
            exporter.ExportByTemplate<ClientExportDto>(tempFile, singleData, templateContent)
                    .GetAwaiter().GetResult();

            var bytes = File.ReadAllBytes(tempFile);
            File.Delete(tempFile);
            return bytes;
        }


        // ============================================================
        // HTML (ручная генерация)
        // ============================================================
        private byte[] ExportToHtml(List<ClientExportDto> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            sb.AppendLine("<h1>Клиенты</h1>");
            sb.AppendLine("<table border='1' cellpadding='5'>");
            sb.AppendLine("<tr><th>ID</th><th>Имя</th><th>Телефон</th><th>Email</th><th>Компания</th><th>Статус</th></tr>");

            foreach (var client in data)
            {
                sb.AppendLine($"<tr><td>{client.Id}</td><td>{client.Name}</td><td>{client.Phone}</td><td>{client.Email}</td><td>{client.Company}</td><td>{client.Status}</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ============================================================
        // WORD (шаблон — путь к файлу .docx)
        // ============================================================

        private byte[] ExportWord(List<ClientExportDto> data, string? templatePath)
        {
            if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
                throw new FileNotFoundException("Файл шаблона для Word не найден.", templatePath);

            var tempFile = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.docx");
            var exporter = new WordExporter();

            exporter.ExportByTemplate(tempFile, data.First(), templatePath).GetAwaiter().GetResult();

            var bytes = File.ReadAllBytes(tempFile);
            File.Delete(tempFile);
            return bytes;
        }
    }
}