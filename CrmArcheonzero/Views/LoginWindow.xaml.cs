using System;
using System.Windows;
using System.Windows.Controls;
using CrmArcheonzero.Data;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            UsernameBox.Text = "admin";
            PasswordBox.Password = "admin123";
        }
        private bool _isConnected = false;

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var provider = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Sqlite";
                var connectionString = _authService.GetConnectionString(provider);

                DbContextFactory.SetProvider(provider, connectionString);
                var context = DbContextFactory.GetDbContext();
                _authService.SetContext(context);

                _isConnected = true;
                UsernameBox.IsEnabled = true;
                PasswordBox.IsEnabled = true;
                LoginButton.IsEnabled = true;

                ProviderComboBox.IsEnabled = false;
                (sender as Button).IsEnabled = false;

                UsernameBox.Focus();
                UsernameBox.Text = "admin";
                PasswordBox.Password = "admin123";

                MessageBox.Show($"Подключение к {provider} установлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoginWindow.Connect_Click");
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = UsernameBox.Text?.Trim();
                var password = PasswordBox.Password;
                var provider = (ProviderComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Sqlite";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_authService.Login(username, password, provider))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordBox.Password = "";
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoginWindow.Login_Click");
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}