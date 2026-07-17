//using CrmArcheonzero.Models;
//using OfficeOpenXml;
//using System.Collections.Generic;
//using System.IO;

//namespace CrmArcheonzero.Services
//{
//    public class ExcelExportService
//    {
//        public void ExportClients(List<Client> clients, string filePath)
//        {
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//            using var package = new ExcelPackage();

//            // ============================================================
//            // ЛИСТ 1: КЛИЕНТЫ
//            // ============================================================
//            var clientSheet = package.Workbook.Worksheets.Add("Клиенты");
//            string[] headers = {
//        "ID", "Имя", "Телефон", "Email", "Компания", "Статус",
//        "Источник", "Теги", "Дата рождения", "Ответственный", "Заметки"
//    };

//            for (int i = 0; i < headers.Length; i++)
//            {
//                clientSheet.Cells[1, i + 1].Value = headers[i];
//                clientSheet.Cells[1, i + 1].Style.Font.Bold = true;
//            }

//            for (int i = 0; i < clients.Count; i++)
//            {
//                var c = clients[i];
//                clientSheet.Cells[i + 2, 1].Value = c.Id;
//                clientSheet.Cells[i + 2, 2].Value = c.Name;
//                clientSheet.Cells[i + 2, 3].Value = c.Phone;
//                clientSheet.Cells[i + 2, 4].Value = c.Email;
//                clientSheet.Cells[i + 2, 5].Value = c.Company;
//                clientSheet.Cells[i + 2, 6].Value = c.Status;
//                clientSheet.Cells[i + 2, 7].Value = c.Source;
//                clientSheet.Cells[i + 2, 8].Value = c.Tags;
//                clientSheet.Cells[i + 2, 9].Value = c.Birthday?.ToString("dd.MM.yyyy");
//                clientSheet.Cells[i + 2, 10].Value = c.AssignedUser?.FullName;
//                clientSheet.Cells[i + 2, 11].Value = c.Notes;
//            }
//            clientSheet.Cells.AutoFitColumns();

//            // ============================================================
//            // ЛИСТ 2: ЗАДАЧИ
//            // ============================================================
//            var taskSheet = package.Workbook.Worksheets.Add("Задачи");
//            taskSheet.Cells[1, 1].Value = "Клиент";
//            taskSheet.Cells[1, 2].Value = "Задача";
//            taskSheet.Cells[1, 3].Value = "Описание";
//            taskSheet.Cells[1, 4].Value = "Срок";
//            taskSheet.Cells[1, 5].Value = "Приоритет";
//            taskSheet.Cells[1, 6].Value = "Статус";

//            int taskRow = 2;
//            foreach (var client in clients)
//            {
//                foreach (var task in client.Tasks)
//                {
//                    taskSheet.Cells[taskRow, 1].Value = client.Name;
//                    taskSheet.Cells[taskRow, 2].Value = task.Title;
//                    taskSheet.Cells[taskRow, 3].Value = task.Description;
//                    taskSheet.Cells[taskRow, 4].Value = task.DueDate.ToString("dd.MM.yyyy");
//                    taskSheet.Cells[taskRow, 5].Value = task.Priority;
//                    taskSheet.Cells[taskRow, 6].Value = task.IsCompleted ? "Выполнена" : "Активна";
//                    taskRow++;
//                }
//            }
//            taskSheet.Cells.AutoFitColumns();

//            // ============================================================
//            // ЛИСТ 3: ВЗАИМОДЕЙСТВИЯ
//            // ============================================================
//            var interactionSheet = package.Workbook.Worksheets.Add("Взаимодействия");
//            interactionSheet.Cells[1, 1].Value = "Клиент";
//            interactionSheet.Cells[1, 2].Value = "Дата";
//            interactionSheet.Cells[1, 3].Value = "Тип";
//            interactionSheet.Cells[1, 4].Value = "Описание";
//            interactionSheet.Cells[1, 5].Value = "Результат";

//            int interactionRow = 2;
//            foreach (var client in clients)
//            {
//                foreach (var interaction in client.Interactions)
//                {
//                    interactionSheet.Cells[interactionRow, 1].Value = client.Name;
//                    interactionSheet.Cells[interactionRow, 2].Value = interaction.Date.ToString("dd.MM.yyyy HH:mm");
//                    interactionSheet.Cells[interactionRow, 3].Value = interaction.Type;
//                    interactionSheet.Cells[interactionRow, 4].Value = interaction.Description;
//                    interactionSheet.Cells[interactionRow, 5].Value = interaction.Outcome;
//                    interactionRow++;
//                }
//            }
//            interactionSheet.Cells.AutoFitColumns();
//            // ============================================================
//            // ЛИСТ 4: ЗАМЕТКИ
//            // ============================================================
//            var noteSheet = package.Workbook.Worksheets.Add("Заметки");
//            noteSheet.Cells[1, 1].Value = "Клиент";
//            noteSheet.Cells[1, 2].Value = "Заметка";
//            noteSheet.Cells[1, 3].Value = "Дата";

//            int noteRow = 2;
//            foreach (var client in clients)
//            {
//                foreach (var note in client.ClientNotes)
//                {
//                    noteSheet.Cells[noteRow, 1].Value = client.Name;
//                    noteSheet.Cells[noteRow, 2].Value = note.Content;
//                    noteSheet.Cells[noteRow, 3].Value = note.CreatedAt.ToString("dd.MM.yyyy HH:mm");
//                    noteRow++;
//                }
//            }
//            noteSheet.Cells.AutoFitColumns();
//            // ============================================================
//            // СОХРАНЕНИЕ
//            // ============================================================
//            var file = new FileInfo(filePath);
//            package.SaveAs(file);
//        }
//    }
//}