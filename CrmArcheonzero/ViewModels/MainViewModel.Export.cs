using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CrmArcheonzero.Services;
using Microsoft.Win32;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // ЭКСПОРТ И БЭКАП
        // ============================================================

        private async void ExportToExcel()
        {
            try
            {
                IsLoading = true;

                // Диалог выбора места сохранения
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    DefaultExt = "xlsx",
                    FileName = $"Clients_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() != true)
                    return; // Пользователь отменил

                var clients = await Task.Run(() => _clientService.GetAll(false));
                var excelService = new ExcelExportService();

                // Передаём путь в сервис (нужно будет модифицировать ExcelExportService)
                await Task.Run(() => excelService.ExportClients(clients, saveDialog.FileName));

                MessageBox.Show($"Экспорт в Excel выполнен!\nФайл: {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ExportToExcel");
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ExportToPdf()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Выберите клиента для экспорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = $"Клиент_{SelectedClient.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var pdfData = await Task.Run(() => _pdfService.GenerateClientCard(SelectedClient));
                    System.IO.File.WriteAllBytes(saveDialog.FileName, pdfData);
                    MessageBox.Show("PDF отчёт сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ExportToPdf");
                MessageBox.Show($"Ошибка экспорта в PDF: {ex.Message}\n\nПодробности записаны в лог.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BackupToCloud()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db",
                    DefaultExt = "db",
                    FileName = $"crm_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.Copy("crm.db", saveDialog.FileName, true);
                    MessageBox.Show($"Резервная копия сохранена:\n{saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "BackupToCloud");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreFromCloud()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Backup files (*.db)|*.db",
                Title = "Выберите файл резервной копии"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(openDialog.FileName, "crm.db", true);
                    LoadClients();
                    MessageBox.Show("База данных восстановлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "RestoreFromCloud");
                    MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}