using System;
using System.Collections.ObjectModel;
using System.Windows;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                OnPropertyChanged();

                if (value != null)
                {
                    OpenUserEditForm(value);
                }
                else
                {
                    CloseUserEditForm();
                }

                (SaveUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ClearUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private User _editableUser = new();
        public User EditableUser
        {
            get => _editableUser;
            set { _editableUser = value; OnPropertyChanged(); }
        }

        private bool _isUserEditMode;
        public bool IsUserEditMode
        {
            get => _isUserEditMode;
            set { _isUserEditMode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles { get; } = new() { "User", "Manager", "SuperManager", "Admin" };

        public void LoadUsers()
        {
            try
            {
                var users = _userService.GetAllUsers();
                Users = new ObservableCollection<User>(users);
                OnPropertyChanged(nameof(Users));
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoadUsers");
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenUserEditForm(User user)
        {
            EditableUser = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive
            };
            IsUserEditMode = true;
            OnPropertyChanged(nameof(EditableUser));
        }

        private void CloseUserEditForm()
        {
            IsUserEditMode = false;
            EditableUser = new User();
        }

        private void SaveUser()
        {
            if (EditableUser == null || EditableUser.Id == 0) return;

            try
            {
                _userService.UpdateUser(EditableUser);

                // ✅ Если мы редактируем сами себя — обновляем CurrentUser
                if (CurrentUser != null && CurrentUser.Id == EditableUser.Id)
                {
                    CurrentUser.Username = EditableUser.Username;
                    CurrentUser.FullName = EditableUser.FullName;
                    CurrentUser.Email = EditableUser.Email;
                    CurrentUser.Role = EditableUser.Role;
                    // Если нужно — обновить и другие поля

                    // Уведомляем UI, что CurrentUser изменился
                    OnPropertyChanged(nameof(CurrentUser));
                }

                LoadUsers();
                CloseUserEditForm();
                SelectedUser = null;
                MessageBox.Show("Пользователь обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "SaveUser");
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                MessageBox.Show("Не удалось определить текущего пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsAdmin)
            {
                MessageBox.Show("Только администратор может удалять пользователей.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedUser.Id == _userService.GetCurrentUser().Id)
            {
                MessageBox.Show("Вы не можете удалить самого себя.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Удалить пользователя {SelectedUser.Username}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (SelectedUser.Role == "Admin")
            {
                MessageBox.Show("Вы не можете удалить другого администратора.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.DeleteUser(SelectedUser.Id);
                    LoadUsers();
                    CloseUserEditForm();
                    SelectedUser = null;
                    MessageBox.Show("Пользователь удалён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "DeleteUser");
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearUser()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("Есть несохранённые изменения. Очистить форму?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }

            CloseUserEditForm();
            SelectedUser = null;
        }

        private bool CanSaveUser() => EditableUser != null && EditableUser.Id != 0;
        private bool CanDeleteUser() => SelectedUser != null && IsAdmin;
        private bool CanClearUser() => IsUserEditMode;

        private void ToggleUserActive(object? parameter)
        {
            if (parameter is not User user) return;

            try
            {
                user.IsActive = !user.IsActive;
                _userService.UpdateUser(user);
                LoadUsers();
                LoggerService.LogAction("ToggleUserActive", $"Пользователь {user.Username} стал {(user.IsActive ? "активен" : "неактивен")}");
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ToggleUserActive");
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}