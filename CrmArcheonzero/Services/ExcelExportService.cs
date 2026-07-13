using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Services
{
    public class ExcelExportService
    {
        public void ExportClients(List<Client> clients)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Клиенты");

            var headers = new[] { "ID", "Имя", "Телефон", "Email", "Компания", "Статус", "Создан" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int i = 0; i < clients.Count; i++)
            {
                var c = clients[i];
                worksheet.Cells[i + 2, 1].Value = c.Id;
                worksheet.Cells[i + 2, 2].Value = c.Name;
                worksheet.Cells[i + 2, 3].Value = c.Phone;
                worksheet.Cells[i + 2, 4].Value = c.Email;
                worksheet.Cells[i + 2, 5].Value = c.Company;
                worksheet.Cells[i + 2, 6].Value = c.Status;
                worksheet.Cells[i + 2, 7].Value = c.CreatedAt.ToString("dd.MM.yyyy");
            }

            worksheet.Cells.AutoFitColumns();
            var file = new FileInfo("Clients.xlsx");
            package.SaveAs(file);
        }
        public void ExportClients(List<Client> clients, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            // ... заполнение данных ...
            var file = new FileInfo(filePath);
            package.SaveAs(file);
        }
    }
}