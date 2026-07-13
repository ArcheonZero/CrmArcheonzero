using System;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // УПРАВЛЕНИЕ ВИДИМОСТЬЮ
        // ============================================================

        private bool _usersTabVisible;
        private bool _userManagementPanelVisible;
        private bool _addUserButtonVisible;
        private bool _recycleBinTabVisible;
        private bool _profileButtonVisible;

        public bool UsersTabVisible
        {
            get => _usersTabVisible;
            set { _usersTabVisible = value; OnPropertyChanged(); }
        }

        public bool UserManagementPanelVisible
        {
            get => _userManagementPanelVisible;
            set { _userManagementPanelVisible = value; OnPropertyChanged(); }
        }

        public bool AddUserButtonVisible
        {
            get => _addUserButtonVisible;
            set { _addUserButtonVisible = value; OnPropertyChanged(); }
        }

        public bool RecycleBinTabVisible
        {
            get => _recycleBinTabVisible;
            set { _recycleBinTabVisible = value; OnPropertyChanged(); }
        }

        public bool ProfileButtonVisible
        {
            get => _profileButtonVisible;
            set { _profileButtonVisible = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Обновляет видимость всех элементов интерфейса в зависимости от роли пользователя
        /// </summary>
        public void UpdateVisibility()
        {
            var isAdmin = IsAdmin;
            var isAuthenticated = IsAuthenticated;

            // Вкладка "Пользователи" — видна всем авторизованным
            UsersTabVisible = isAuthenticated;

            // Панель управления пользователями (поля + кнопка) — только админу
            UserManagementPanelVisible = isAdmin;
            AddUserButtonVisible = isAdmin;

            // Корзина — видна всем авторизованным
            RecycleBinTabVisible = isAuthenticated;

            // Кнопка "Профиль" — пока скрыта, можно включить позже
            ProfileButtonVisible = false;
        }
    }
}