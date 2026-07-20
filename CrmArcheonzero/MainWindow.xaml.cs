using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrmArcheonzero.Services;
using CrmArcheonzero.Views;

namespace CrmArcheonzero
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(_authService);
            loginWindow.Owner = this;

            if (loginWindow.ShowDialog() == true)
            {
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "✅ Выход";
                    button.Background = new SolidColorBrush(Colors.Green);
                }

                if (DataContext is ViewModels.MainViewModel vm)
                {
                    _ = vm.LoadClientsAsync();
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newUsername = FindName("NewUsername") as TextBox;
                var newPassword = FindName("NewPassword") as PasswordBox;
                var newEmail = FindName("NewEmail") as TextBox;
                var newRole = FindName("NewRole") as ComboBox;

                if (newUsername == null || newPassword == null || newEmail == null || newRole == null)
                {
                    MessageBox.Show("Ошибка инициализации формы. Перезапустите приложение.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var username = newUsername.Text?.Trim();
                var password = newPassword.Password;
                var email = newEmail.Text?.Trim();
                var role = (newRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Введите имя пользователя!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    newUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    newPassword.Focus();
                    return;
                }

                if (password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    newPassword.Focus();
                    return;
                }

                if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                {
                    MessageBox.Show("Введите корректный email!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    newEmail.Focus();
                    return;
                }

                if (_authService.CreateUser(username, password, email, username, role))
                {
                    MessageBox.Show($"Пользователь '{username}' создан с ролью {role}!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    newUsername.Text = "";
                    newPassword.Password = "";
                    newEmail.Text = "";
                    newRole.SelectedIndex = 0;

                    if (DataContext is ViewModels.MainViewModel vm)
                    {
                        vm.LoadUsers();
                    }
                }
                else
                {
                    MessageBox.Show($"Пользователь с именем '{username}' уже существует!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    newUsername.Focus();
                    newUsername.SelectAll();
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "MainWindow.AddUser_Click");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void TopPanelView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}