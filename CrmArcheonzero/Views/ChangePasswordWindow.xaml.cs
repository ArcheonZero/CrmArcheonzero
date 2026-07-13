using System;
using System.Windows;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly AuthService _authService;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
            
            Loaded += (s, e) => OldPasswordBox.Focus();
            
            OldPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    NewPasswordBox.Focus();
            };
            
            NewPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    ConfirmPasswordBox.Focus();
            };
            
            ConfirmPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    Change_Click(s, e);
            };
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    MessageBox.Show("Пользователь не авторизован!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var oldPassword = OldPasswordBox.Password;
                var newPassword = NewPasswordBox.Password;
                var confirmPassword = ConfirmPasswordBox.Password;

                if (string.IsNullOrEmpty(oldPassword))
                {
                    MessageBox.Show("Введите текущий пароль!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    OldPasswordBox.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show("Введите новый пароль!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NewPasswordBox.Focus();
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageBox.Show("Новый пароль должен содержать минимум 6 символов!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NewPasswordBox.Focus();
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ConfirmPasswordBox.Focus();
                    return;
                }

                if (oldPassword == newPassword)
                {
                    MessageBox.Show("Новый пароль должен отличаться от текущего!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NewPasswordBox.Focus();
                    return;
                }

                if (_authService.ChangePassword(user.Id, oldPassword, newPassword))
                {
                    MessageBox.Show("Пароль успешно изменён!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный текущий пароль!", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    OldPasswordBox.Password = "";
                    OldPasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ChangePasswordWindow.Change_Click");
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", 
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}