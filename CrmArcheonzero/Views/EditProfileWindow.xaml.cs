using System;
using System.Windows;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.Views
{
    public partial class EditProfileWindow : Window
    {
        private readonly AuthService _authService;

        public EditProfileWindow()
        {
            InitializeComponent();
            _authService = new AuthService();

            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                FullNameBox.Text = user.FullName;
                EmailBox.Text = user.Email;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newName = FullNameBox.Text?.Trim();
                var newEmail = EmailBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(newName))
                {
                    StatusText.Text = "Имя не может быть пустым!";
                    return;
                }

                if (string.IsNullOrWhiteSpace(newEmail) || !IsValidEmail(newEmail))
                {
                    StatusText.Text = "Введите корректный Email!";
                    return;
                }

                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    StatusText.Text = "Пользователь не авторизован!";
                    return;
                }

                if (_authService.UpdateProfile(user.Id, newName, newEmail))
                {
                    MessageBox.Show("Профиль обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    StatusText.Text = "Ошибка обновления профиля!";
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "EditProfileWindow.Save_Click");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}