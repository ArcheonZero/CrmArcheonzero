using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CrmArcheonzero.DTO;
using CrmArcheonzero.Services;
using Microsoft.Win32;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // ЭКСПОРТ (единый метод)
        // ============================================================

        private async void Export()
        {
            
            try
            {
                IsLoading = true;
                var clients = await Task.Run(() => _clientService.GetAll(false));
                var exportData = clients.Select(c => new ClientExportDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Company = c.Company,
                    Status = c.Status,
                    Source = c.Source,
                    Tags = c.Tags,
                    Birthday = c.Birthday,
                    AssignedUser = c.AssignedUser?.FullName,
                    Notes = c.Notes
                }).ToList();

                var exportService = new ExportService();
                string? templatePath = SelectedExportFormat switch
                {
                    "pdf" => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "PdfTemplate.html"),
                    "word" => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "WordTemplate.docx"),
                    _ => null
                };
                if (!string.IsNullOrEmpty(templatePath) && !File.Exists(templatePath))
                {
                    MessageBox.Show($"Файл шаблона не найден:\n{templatePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var fileBytes = exportService.ExportClients(exportData, SelectedExportFormat, templatePath);

                var saveDialog = new SaveFileDialog
                {
                    Filter = GetFilter(SelectedExportFormat),
                    DefaultExt = SelectedExportFormat,
                    FileName = $"Clients_{DateTime.Now:yyyyMMdd_HHmmss}.{SelectedExportFormat}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllBytes(saveDialog.FileName, fileBytes);
                    MessageBox.Show($"Экспорт в {SelectedExportFormat.ToUpper()} выполнен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "Export");
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============================================================
        // ФОРМАТЫ ЭКСПОРТА
        // ============================================================

        private List<string> _exportFormats = new() { "xlsx", "csv", "html" };
        public List<string> ExportFormats
        {
            get => _exportFormats;
            set
            {
                if (_exportFormats == value) return;
                _exportFormats = value;
                OnPropertyChanged();
            }
        }

        private string _selectedExportFormat = "xlsx";
        public string SelectedExportFormat
        {
            get => _selectedExportFormat;
            set
            {
                if (_selectedExportFormat == value) return;
                _selectedExportFormat = value;
                OnPropertyChanged();
            }
        }




        private bool CanExportCard(string? format) => SelectedClient != null && IsAuthenticated;

        private async void ExportCard(string? format)
        {
            if (SelectedClient == null || string.IsNullOrEmpty(format)) return;

            try
            {
                IsLoading = true;
                var exportData = new List<ClientExportDto>
        {
            new ClientExportDto
            {
                Id = SelectedClient.Id,
                Name = SelectedClient.Name,
                Phone = SelectedClient.Phone,
                Email = SelectedClient.Email,
                Company = SelectedClient.Company,
                Status = SelectedClient.Status,
                Source = SelectedClient.Source,
                Tags = SelectedClient.Tags,
                Birthday = SelectedClient.Birthday,
                AssignedUser = SelectedClient.AssignedUser?.FullName,
                Notes = SelectedClient.Notes
            }
        };

                var exportService = new ExportService();
                var fileBytes = exportService.ExportClients(exportData, format);

                var saveDialog = new SaveFileDialog
                {
                    Filter = GetFilter(format),
                    DefaultExt = format,
                    FileName = $"Клиент_{SelectedClient.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{format}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllBytes(saveDialog.FileName, fileBytes);
                    MessageBox.Show($"Карточка экспортирована в {format.ToUpper()}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, $"ExportCard_{format}");
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================================

        private string GetFilter(string format) => format.ToLower() switch
        {
            "xlsx" => "Excel files (*.xlsx)|*.xlsx",
            "csv" => "CSV files (*.csv)|*.csv",
            "html" => "HTML files (*.html)|*.html",
            _ => "All files (*.*)|*.*"
        };

        // ============================================================
        // БЭКАП
        // ============================================================

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