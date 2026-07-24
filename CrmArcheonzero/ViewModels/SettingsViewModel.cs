using CrmArcheonzero.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CrmArcheonzero.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _selectedTheme = "Light";

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme == value) return;
                _selectedTheme = value;
                OnPropertyChanged();
                ApplyTheme(value);
            }
        }

        // Свойства для упрощения привязки RadioButton
        public bool IsLightTheme
        {
            get => SelectedTheme == "Light";
            set => SelectedTheme = value ? "Light" : SelectedTheme;
        }

        public bool IsDarkTheme
        {
            get => SelectedTheme == "Dark";
            set => SelectedTheme = value ? "Dark" : SelectedTheme;
        }

        public bool IsBrightTheme
        {
            get => SelectedTheme == "Bright";
            set => SelectedTheme = value ? "Bright" : SelectedTheme;
        }

        private void ApplyTheme(string themeName)
        {
            try
            {
                // Формируем путь к файлу темы
                var uri = new Uri($"/Views/Styles/Themes/{themeName}Theme.xaml", UriKind.Relative);

                // Загружаем новый словарь ресурсов
                var newTheme = new ResourceDictionary { Source = uri };

                // Находим и удаляем старую тему из ресурсов приложения
                var oldTheme = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

                if (oldTheme != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldTheme);
                }

                // Добавляем новую тему
                Application.Current.Resources.MergedDictionaries.Add(newTheme);

                LoggerService.LogAction("SettingsViewModel", $"Тема изменена на {themeName}");
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "ApplyTheme");
                MessageBox.Show($"Ошибка при смене темы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}