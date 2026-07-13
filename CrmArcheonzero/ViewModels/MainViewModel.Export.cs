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
                var clients = await Task.Run(() => _clientService.GetAll(false));
                var excelService = new ExcelExportService();
                await Task.Run(() => excelService.ExportClients(clients));
                MessageBox.Show("Экспорт в Excel выполнен!\nФайл: Clients.xlsx", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ExportToPdf()
        {
            if (SelectedClient == null) return;
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = $"Клиент_{SelectedClient.Name}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var pdfData = _pdfService.GenerateClientReport(SelectedClient);
                    File.WriteAllBytes(saveDialog.FileName, pdfData);
                    MessageBox.Show("PDF отчёт сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ExportToPdf");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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