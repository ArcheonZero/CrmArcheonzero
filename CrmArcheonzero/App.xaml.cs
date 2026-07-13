using System;
using System.Threading.Tasks;
using System.Windows;
using CrmArcheonzero.Services;
using CrmArcheonzero.Views;

namespace CrmArcheonzero
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoggerService.CleanOldLogs();

            DispatcherUnhandledException += (s, args) =>
            {
                LoggerService.LogError(args.Exception, "Глобальный перехват (UI)");
                MessageBox.Show("Произошла ошибка. Подробности записаны в лог.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    LoggerService.LogError(ex, "AppDomain.UnhandledException");
            };

            var mainWindow = new MainWindow();
            mainWindow.Show();
            await Task.Delay(10);
            await LoadDataAsync(mainWindow);
        }

        private async Task LoadDataAsync(MainWindow mainWindow)
        {
            try
            {
                var vm = mainWindow.DataContext as ViewModels.MainViewModel;
                if (vm == null) return;
                await vm.LoadClientsAsync();
                await vm.LoadAdditionalDataAsync();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "Фоновая загрузка");
            }
        }
    }
}