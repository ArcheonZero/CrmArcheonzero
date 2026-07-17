using System;
using System.Collections.ObjectModel;
using System.Windows;
using CrmArcheonzero.Models;
using CrmArcheonzero.Views;


namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // АВТОРИЗАЦИЯ
        // ============================================================

        private void ShowLogin()
        {
            var loginWindow = new LoginWindow(_userService.GetAuthService());
            loginWindow.Owner = Application.Current.MainWindow;
            if (loginWindow.ShowDialog() == true)
            {
                IsAuthenticated = true;
                (ExportCardCommand as RelayCommand<string>)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsAuthenticated));

                HasUnsavedChanges = false;
                LoadClients();

                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(IsSuperManager));
                UpdateVisibility();
                RefreshCommands();

                var user = _userService.GetCurrentUser();
                var roleDisplay = user?.Role switch
                {
                    "Admin" => "(Администратор)",
                    "SuperManager" => "(Старший менеджер)",
                    "Manager" => "(Менеджер)",
                    _ => "(Пользователь)"
                };
                MessageBox.Show($"Добро пожаловать, {user?.FullName} {roleDisplay}!",
                    "Приветствие", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Logout()
{
    if (HasUnsavedChanges)
    {
        var result = MessageBox.Show("Есть несохранённые изменения. Выйти?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No) return;
    }

    _userService.Logout();
    IsAuthenticated = false;
    IsEditMode = false;
    HasUnsavedChanges = false;
    Clients = new ObservableCollection<Client>();
    ChatMessages = new ObservableCollection<ChatMessage>(); // <-- ДОБАВИТЬ

    UpdateVisibility();
    RefreshCommands();

    MessageBox.Show("Вы вышли из системы", "Выход", MessageBoxButton.OK, MessageBoxImage.Information);
}

        private void ShowChangePassword()
        {
            if (!IsAuthenticated)
            {
                MessageBox.Show("Вы не авторизованы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var window = new ChangePasswordWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void ShowUserManagement()
        {
            SelectedTabIndex = 3;
        }
    }
}