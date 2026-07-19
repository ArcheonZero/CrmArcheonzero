using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.Win32;
using Magicodes.ExporterAndImporter.Excel;
using CrmArcheonzero.Models;
using CrmArcheonzero.DTO;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // МЕТОД ИМПОРТА
        // ============================================================
        private async void ImportFromExcel()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Выберите файл для импорта"
                };

                if (dialog.ShowDialog() != true) return;

                IsLoading = true;

                // Импорт через Magicodes.IE
                var importer = new ExcelImporter();
                var importResult = await Task.Run(() => importer.Import<ClientImportDto>(dialog.FileName, null));

                if (importResult.HasError)
                {
                    var errors = string.Join("\n", importResult.RowErrors);
                    MessageBox.Show($"Ошибки импорта:\n{errors}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var imported = 0;
                var updated = 0;
                foreach (var dto in importResult.Data)
                {
                    // Проверка на дубли (по телефону или email)
                    var existing = _clientService.GetByPhoneOrEmail(dto.Phone, dto.Email);
                    if (existing != null)
                    {
                        // Обновляем существующего клиента
                        existing.Name = dto.Name;
                        existing.Phone = dto.Phone;
                        existing.Email = dto.Email;
                        existing.Company = dto.Company;
                        existing.Status = dto.Status ?? "Lead";
                        existing.Source = dto.Source;
                        existing.Tags = dto.Tags;
                        existing.Birthday = dto.Birthday;
                        existing.Notes = dto.Notes;

                        _clientService.Update(existing);
                        updated++;
                    }
                    else
                    {
                        // Добавляем нового клиента
                        var client = new Client
                        {
                            Name = dto.Name,
                            Phone = dto.Phone,
                            Email = dto.Email,
                            Company = dto.Company,
                            Status = dto.Status ?? "Lead",
                            Source = dto.Source,
                            Tags = dto.Tags,
                            Birthday = dto.Birthday,
                            Notes = dto.Notes,
                            CreatedAt = DateTime.UtcNow
                        };
                        _clientService.Add(client);
                        imported++;
                    }

                }

                MessageBox.Show($"Импортировано: {imported} новых, обновлено: {updated}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadClients();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ImportFromExcel");
                MessageBox.Show($"Ошибка импорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}