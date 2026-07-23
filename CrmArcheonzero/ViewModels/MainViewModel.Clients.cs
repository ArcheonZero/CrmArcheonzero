using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;
using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.EntityFrameworkCore;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // КЛИЕНТЫ — CRUD, ПОИСК, ФИЛЬТР, СТАТИСТИКА
        // ============================================================

        public async Task LoadClientsAsync()
        {
            if (!IsAuthenticated) return;
            IsLoading = true;
            try
            {
                var list = await _clientService.GetAllAsync(false);
                Clients = new ObservableCollection<Client>(list);
                ApplyFilter(); // <-- добавить, если ещё нет
                UpdateChart();
                LoadUsers();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoadClientsAsync");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadClients() => _ = LoadClientsAsync();

        private async void Search()
        {
            if (!IsAuthenticated) return;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadClients();
                return;
            }
            IsLoading = true;
            try
            {
                var results = await Task.Run(() => _clientService.Search(SearchText));
                Clients = new ObservableCollection<Client>(results);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "Search");
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            if (!IsAuthenticated || Clients == null) return;

            try
            {
                var all = _clientService.GetAll(false);

                // Фильтр по статусу
                if (StatusFilter != "Все")
                {
                    all = _clientService.ApplyFilter(all, StatusFilter);
                }

                // Фильтр "Мои клиенты"
                if (ShowMyClientsOnly && CurrentUser != null)
                {
                    all = all.Where(c => c.AssignedUserId == CurrentUser.Id).ToList();
                }

                Clients = new ObservableCollection<Client>(all);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ApplyFilter");
            }
        }

        private void UpdateChart()
        {
            if (!IsAuthenticated) return;
            try
            {
                var stats = _clientService.GetStatistics(false);
                LoggerService.LogAction("UpdateChart", $"Stats: Total={stats["Total"]}, Active={stats["Active"]}, Inactive={stats["Inactive"]}, Lead={stats["Lead"]}");
                // Обновляем цифры
                ActiveCount = stats["Active"];
                LeadCount = stats["Lead"];
                InactiveCount = stats["Inactive"];

                // === ГЛАВНОЕ ИЗМЕНЕНИЕ ===
                // Создаём НОВЫЙ SeriesCollection вместо изменения существующего
                ChartSeries = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Клиенты",
                Values = new ChartValues<int> { stats["Active"], stats["Inactive"], stats["Lead"] }
            }
        };

                // Обновляем подписи осей (если они не заданы)
                ChartLabels = new[] { "Active", "Inactive", "Lead" };
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "UpdateChart");
            }
        }

        private void LoadClientDetails(int clientId)
        {
            if (!IsAuthenticated) return;
            try
            {
                var client = _clientService.GetById(clientId);
                if (client != null)
                {
                    Interactions = new ObservableCollection<Interaction>(client.Interactions.OrderByDescending(i => i.Date));
                    Tasks = new ObservableCollection<ClientTask>(client.Tasks.OrderBy(t => t.DueDate));
                    Notes = new ObservableCollection<Note>(client.ClientNotes.OrderByDescending(n => n.CreatedAt));
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoadClientDetails");
            }
        }

        private void OpenEditForm(Client? client = null)
        {
            if (!IsAuthenticated) return;

            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("Есть несохранённые изменения. Продолжить?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            if (client == null)
            {
                EditableClient = new Client
                {
                    CreatedAt = DateTime.UtcNow,
                    Status = "Lead",
                    AssignedUserId = _userService.GetCurrentUser()?.Id
                };
                SelectedClient = null;
                Interactions = new ObservableCollection<Interaction>();
                Tasks = new ObservableCollection<ClientTask>();
                Notes = new ObservableCollection<Note>();
            }
            else
            {
                EditableClient = new Client
                {
                    Id = client.Id,
                    Name = client.Name,
                    Phone = client.Phone,
                    Email = client.Email,
                    Status = client.Status,
                    Notes = client.Notes,
                    Company = client.Company,
                    Position = client.Position,
                    Address = client.Address,
                    Source = client.Source,
                    CreatedAt = client.CreatedAt,
                    LastContact = client.LastContact,
                    Birthday = client.Birthday,
                    AssignedUserId = client.AssignedUserId,
                    Tags = client.Tags
                };
                SelectedClient = client;
                LoadClientDetails(client.Id);
            }

            IsEditMode = true;
            HasUnsavedChanges = false;
            SelectedTabIndex = 2;
            OnPropertyChanged(nameof(EditableClient));
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void CloseEditForm()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("Есть несохранённые изменения. Закрыть форму?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            IsEditMode = false;
            HasUnsavedChanges = false;
            EditableClient = new Client();
            SelectedClient = null;
            Interactions = new ObservableCollection<Interaction>();
            Tasks = new ObservableCollection<ClientTask>();
            Notes = new ObservableCollection<Note>();

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void AddClient() => OpenEditForm(null);

        private void SaveClient()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditableClient.Name))
                {
                    MessageBox.Show("Введите имя клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(EditableClient.Email))
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(EditableClient.Email);
                        if (addr.Address != EditableClient.Email)
                            throw new FormatException();
                    }
                    catch
                    {
                        MessageBox.Show("Введите корректный Email!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (_clientService.ClientExists(EditableClient.Email, EditableClient.Id))
                    {
                        MessageBox.Show("Клиент с таким Email уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(EditableClient.Phone))
                {
                    var digits = new string(EditableClient.Phone.Where(char.IsDigit).ToArray());
                    if (digits.Length < 10 || digits.Length > 15)
                    {
                        MessageBox.Show("Телефон должен содержать от 10 до 15 цифр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (EditableClient.Id == 0)
                {
                    _clientService.Add(EditableClient);
                    LoggerService.LogAction("Создание клиента", $"Клиент {EditableClient.Name} создан");
                    _telegramService?.SendClientNotification(EditableClient.Name, "Добавлен новый клиент");
                }
                else
                {
                    var existing = _clientService.GetById(EditableClient.Id);
                    if (existing == null)
                    {
                        MessageBox.Show("Клиент был удалён другим пользователем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        CloseEditForm();
                        LoadClients();
                        return;
                    }
                    if (!IsAdmin && existing.AssignedUserId != _userService.GetCurrentUser()?.Id)
                    {
                        MessageBox.Show("Вы можете редактировать только своих клиентов.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    existing.Name = EditableClient.Name;
                    existing.Phone = EditableClient.Phone;
                    existing.Email = EditableClient.Email;
                    existing.Status = EditableClient.Status;
                    existing.Notes = EditableClient.Notes;
                    existing.Company = EditableClient.Company;
                    existing.Position = EditableClient.Position;
                    existing.Address = EditableClient.Address;
                    existing.Source = EditableClient.Source;
                    existing.Tags = EditableClient.Tags;
                    existing.LastContact = EditableClient.LastContact;
                    existing.Birthday = EditableClient.Birthday;
                    existing.AssignedUserId = EditableClient.AssignedUserId;

                    _clientService.Update(existing);
                    LoggerService.LogAction("Обновление клиента", $"Клиент {existing.Name} (ID: {existing.Id}) обновлён");
                    _telegramService?.SendClientNotification(existing.Name, "Обновлена информация");
                }

                EditableClient = new Client();
                OnPropertyChanged(nameof(EditableClient));
                SelectedClient = null;
                IsEditMode = false;
                LoadClients();
                RefreshCommands();

                MessageBox.Show("Клиент сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateConcurrencyException)
            {
                MessageBox.Show("Клиент был изменён другим пользователем.", "Конфликт", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadClients();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "SaveClient");
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteClient()
        {
            if (SelectedClient == null) return;

            if (!IsAdmin && SelectedClient.AssignedUserId != _userService.GetCurrentUser()?.Id)
            {
                MessageBox.Show("Вы можете удалять только своих клиентов.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Переместить клиента {SelectedClient.Name} в корзину?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var clientName = SelectedClient.Name;
                    var userId = _userService.GetCurrentUser()?.Id ?? 0;

                    _clientService.SoftDelete(SelectedClient.Id, userId);
                    LoggerService.LogAction("Удаление в корзину", $"Клиент {clientName} (ID: {SelectedClient.Id}) удалён пользователем {_userService.GetCurrentUser()?.Username}");

                    CloseEditForm();
                    LoadClients();
                    LoadDeletedClients();
                    _telegramService?.SendClientNotification(clientName, "Удалён в корзину");
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "DeleteClient");
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            SelectedClient = null;
        }

        private void RestoreClient()
        {
            if (SelectedDeletedClient == null || !SelectedDeletedClient.IsDeleted) return;

            var result = MessageBox.Show($"Восстановить клиента {SelectedDeletedClient.Name}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var clientName = SelectedDeletedClient.Name;
                    _clientService.Restore(SelectedDeletedClient.Id);
                    LoggerService.LogAction("Восстановление из корзины", $"Клиент {clientName} (ID: {SelectedDeletedClient.Id}) восстановлен пользователем {_userService.GetCurrentUser()?.Username}");

                    LoadDeletedClients();
                    LoadClients();
                    SelectedDeletedClient = null;
                    MessageBox.Show($"Клиент {clientName} восстановлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "RestoreClient");
                    MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PermanentDeleteClient()
        {
            if (SelectedDeletedClient == null || !SelectedDeletedClient.IsDeleted) return;

            if (!IsSuperManager)
            {
                MessageBox.Show("Только SuperManager или Admin могут окончательно удалять клиентов.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Окончательно удалить клиента {SelectedDeletedClient.Name}? Это действие нельзя отменить.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var clientName = SelectedDeletedClient.Name;
                    _clientService.PermanentDelete(SelectedDeletedClient.Id);
                    LoggerService.LogAction("Окончательное удаление", $"Клиент {clientName} (ID: {SelectedDeletedClient.Id}) удалён навсегда пользователем {_userService.GetCurrentUser()?.Username}");

                    CloseEditForm();
                    LoadClients();
                    LoadDeletedClients();
                    MessageBox.Show($"Клиент {clientName} удалён навсегда.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "PermanentDeleteClient");
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearForm()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("Есть несохранённые изменения. Очистить форму?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            EditableClient = new Client();
            HasUnsavedChanges = false;

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void ShowBirthdays()
        {
            if (!IsAuthenticated) return;
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var birthdayClients = _clientService.GetClientsWithBirthdayInMonth(currentMonth);

                if (birthdayClients.Count == 0)
                {
                    MessageBox.Show("В этом месяце нет дней рождения.", "Дни рождения", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var message = "🎂 Дни рождения в этом месяце:\n\n";
                foreach (var client in birthdayClients)
                {
                    message += $"• {client.Name} - {client.Birthday!.Value:dd.MM}\n";
                }
                MessageBox.Show(message, "Дни рождения", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ShowBirthdays");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}