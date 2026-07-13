using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.Services
{
    public class PdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateClientReport(Client client)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial")); // <-- ЯВНО УКАЗЫВАЕМ ШРИФТ

                    page.Header()
                        .Text($"Карточка клиента: {client.Name}")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2)
                        .FontFamily("Arial"); // <-- И ЗДЕСЬ

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().Text("Основная информация").FontSize(16).Bold().FontFamily("Arial");
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                });
                                AddRow(table, "Имя:", client.Name);
                                AddRow(table, "Телефон:", client.Phone);
                                AddRow(table, "Email:", client.Email);
                                AddRow(table, "Компания:", client.Company);
                                AddRow(table, "Статус:", client.Status);
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm} | CRM System")
                        .FontFamily("Arial");
                });
            });

            return document.GeneratePdf();
        }

        private void AddRow(TableDescriptor table, string label, string? value)
        {
            table.Cell().Text(label).Bold().FontFamily("Arial");
            table.Cell().Text(value ?? "-").FontFamily("Arial");
        }
    }
}