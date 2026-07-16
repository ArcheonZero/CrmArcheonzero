using System;
using System.Threading.Tasks;   // для Task
using System.Windows;           // для MessageBox
using Microsoft.Win32;          // для OpenFileDialog
using CrmArcheonzero.Services;
using CrmArcheonzero.Models;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
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
                var importService = new ExcelImportService();
                var (clients, tasks, interactions, notes, errors) = 
                    await Task.Run(() => importService.ImportFromExcel(dialog.FileName));

                if (clients.Count == 0)
                {
                    MessageBox.Show("Нет данных для импорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сохранение клиентов
                foreach (var client in clients)
                {
                    await _clientService.Add(client);
                }

                // ... сохранение задач, взаимодействий, заметок ...

                MessageBox.Show($"Импортировано {clients.Count} клиентов.", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadClients();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ImportFromExcel");
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanImportExcel() => IsAuthenticated;
    }
}