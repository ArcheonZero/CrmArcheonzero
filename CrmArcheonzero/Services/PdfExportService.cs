//using System;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using CrmArcheonzero.Models;
//using System.Linq;

//namespace CrmArcheonzero.Services
//{
//    public class PdfExportService
//    {
//        public PdfExportService()
//        {
//            QuestPDF.Settings.License = LicenseType.Community;
//        }

//        public byte[] GenerateClientCard(Client client)
//        {
//            try
//            {
//                QuestPDF.Settings.License = LicenseType.Community;

//                var document = Document.Create(container =>
//                {
//                    container.Page(page =>
//                    {
//                        page.Size(PageSizes.A4);
//                        page.Margin(2, Unit.Centimetre);
//                        page.PageColor(Colors.White);
//                        // ✅ УСТАНАВЛИВАЕМ ШРИФТ ДЛЯ ВСЕГО ДОКУМЕНТА
//                        page.DefaultTextStyle(x => x
//                            .FontFamily("Times New Roman")
//                            .Fallback(fallback => fallback
//                                .FontFamily("Segoe UI Emoji")
//                                .FontFamily("Segoe UI Symbol")
//                            )
//                            .FontSize(12)
//                        );
//                        page.Header()
//                            .Text($"Карточка клиента: {client.Name}")
//                            .FontSize(20)
//                            .Bold()
//                            .FontColor(Colors.Blue.Darken2);

//                        page.Content()
//                            .PaddingVertical(1, Unit.Centimetre)
//                            .Column(x =>
//                            {
//                                x.Spacing(10);

//                                // === ОСНОВНАЯ ИНФОРМАЦИЯ ===
//                                x.Item().Text("Основная информация").FontSize(16).Bold();
//                                x.Item().Table(table =>
//                                {
//                                    table.ColumnsDefinition(columns =>
//                                    {
//                                        columns.RelativeColumn(1);
//                                        columns.RelativeColumn(3);
//                                    });

//                                    AddRow(table, "Имя:", client.Name);
//                                    AddRow(table, "Телефон:", client.Phone);
//                                    AddRow(table, "Email:", client.Email);
//                                    AddRow(table, "Компания:", client.Company);
//                                    AddRow(table, "Статус:", client.Status);
//                                    AddRow(table, "Источник:", client.Source);
//                                    AddRow(table, "Теги:", client.Tags);
//                                    AddRow(table, "Дата рождения:", client.Birthday?.ToString("dd.MM.yyyy"));
//                                    AddRow(table, "Примечания:", client.Notes);
//                                    AddRow(table, "Дата создания:", client.CreatedAt.ToString("dd.MM.yyyy"));
//                                });

//                                // === ЗАДАЧИ ===
//                                if (client.Tasks?.Any() == true)
//                                {
//                                    x.Item().Text("Задачи").FontSize(16).Bold();
//                                    x.Item().Table(table =>
//                                    {
//                                        table.ColumnsDefinition(columns =>
//                                        {
//                                            columns.RelativeColumn(3);
//                                            columns.RelativeColumn(1);
//                                            columns.RelativeColumn(1);
//                                        });

//                                        table.Header(header =>
//                                        {
//                                            header.Cell().Text("Название").Bold();
//                                            header.Cell().Text("Срок").Bold();
//                                            header.Cell().Text("Статус").Bold();
//                                        });

//                                        foreach (var task in client.Tasks)
//                                        {
//                                            table.Cell().Text(task.Title);
//                                            table.Cell().Text(task.DueDate.ToString("dd.MM.yyyy"));
//                                            table.Cell().Text(task.IsCompleted ? "✅ Выполнена" : "⏳ Активна");
//                                        }
//                                    });
//                                }

//                                // === ВЗАИМОДЕЙСТВИЯ ===
//                                if (client.Interactions?.Any() == true)
//                                {
//                                    x.Item().Text("Взаимодействия").FontSize(16).Bold();
//                                    x.Item().Table(table =>
//                                    {
//                                        table.ColumnsDefinition(columns =>
//                                        {
//                                            columns.RelativeColumn(2);
//                                            columns.RelativeColumn(2);
//                                            columns.RelativeColumn(1);
//                                        });

//                                        table.Header(header =>
//                                        {
//                                            header.Cell().Text("Дата").Bold();
//                                            header.Cell().Text("Тип").Bold();
//                                            header.Cell().Text("Описание").Bold();
//                                        });

//                                        foreach (var interaction in client.Interactions)
//                                        {
//                                            table.Cell().Text(interaction.Date.ToString("dd.MM.yyyy HH:mm"));
//                                            table.Cell().Text(interaction.Type);
//                                            table.Cell().Text(interaction.Description);
//                                        }
//                                    });
//                                }
//                                // === ЗАМЕТКИ ===
//                                if (client.ClientNotes?.Any() == true)
//                                {
//                                    x.Item().Text("Заметки").FontSize(16).Bold();
//                                    x.Item().Table(table =>
//                                    {
//                                        table.ColumnsDefinition(columns =>
//                                        {
//                                            columns.RelativeColumn(3);
//                                            columns.RelativeColumn(1);
//                                        });

//                                        table.Header(header =>
//                                        {
//                                            header.Cell().Text("Заметка").Bold();
//                                            header.Cell().Text("Дата").Bold();
//                                        });

//                                        foreach (var note in client.ClientNotes)
//                                        {
//                                            table.Cell().Text(note.Content);
//                                            table.Cell().Text(note.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
//                                        }
//                                    });
//                                }
//                            });

//                        page.Footer()
//                            .AlignCenter()
//                            .Text($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm} | CRM Archeonzero v2.0.1");
//                    });
//                });

//                return document.GeneratePdf();
//            }
//            catch (Exception ex)
//            {
//                LoggerService.LogError(ex, "PdfExportService.GenerateClientCard");
//                throw; // Перебрасываем, чтобы показать сообщение пользователю
//            }
//        }

//        private void AddRow(TableDescriptor table, string label, string? value)
//        {
//            table.Cell().Text(label).Bold().FontFamily("Arial");
//            table.Cell().Text(value ?? "-").FontFamily("Arial");
//        }
//    }
//}