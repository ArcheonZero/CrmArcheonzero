using CrmArcheonzero.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // КОМАНДЫ
        // ============================================================

        public ICommand AddCommand { get; private set; } = null!;
        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand DeleteCommand { get; private set; } = null!;
        public ICommand ClearCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand AddInteractionCommand { get; private set; } = null!;
        public ICommand AddTaskCommand { get; private set; } = null!;
        public ICommand CompleteTaskCommand { get; private set; } = null!;
        public ICommand DeleteTaskCommand { get; private set; } = null!;
        public ICommand AddNoteCommand { get; private set; } = null!;
        public ICommand DeleteNoteCommand { get; private set; } = null!;
        public ICommand ExportExcelCommand { get; private set; } = null!;
        public ICommand ExportPdfCommand { get; private set; } = null!;
        public ICommand ShowBirthdaysCommand { get; private set; } = null!;
        public ICommand SendEmailCommand { get; private set; } = null!;
        public ICommand SendTelegramCommand { get; private set; } = null!;
        public ICommand BackupToCloudCommand { get; private set; } = null!;
        public ICommand RestoreFromCloudCommand { get; private set; } = null!;
        public ICommand LoginCommand { get; private set; } = null!;
        public ICommand LogoutCommand { get; private set; } = null!;
        public ICommand ChangePasswordCommand { get; private set; } = null!;
        public ICommand ShowUserManagementCommand { get; private set; } = null!;
        public ICommand ReassignClientCommand { get; private set; } = null!;
        public ICommand RestoreClientCommand { get; private set; } = null!;
        public ICommand PermanentDeleteClientCommand { get; private set; } = null!;
        public ICommand OpenRecycleBinCommand { get; private set; } = null!;
        public ICommand SaveUserCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand ClearUserCommand { get; private set; }
        //export - magicodes
        public ICommand ExportCommand { get; set; }

        public ICommand ExportCardCommand { get; private set; }
        private RelayCommand? _importCommand;
        public ICommand ImportCommand => _importCommand ??= new RelayCommand(ImportFromExcel, () => IsAuthenticated);
        // ============================================================
        // ИНИЦИАЛИЗАЦИЯ КОМАНД
        // ============================================================

        private void InitializeCommands()
        {
            LoggerService.LogAction("InitializeCommands", "Начало");
            AddCommand = new RelayCommand(AddClient, () => IsAuthenticated);
            SaveCommand = new RelayCommand(SaveClient, () => IsAuthenticated && IsEditMode);
            DeleteCommand = new RelayCommand(DeleteClient, () => IsAuthenticated && SelectedClient != null);
            ClearCommand = new RelayCommand(ClearForm, () => IsAuthenticated && IsEditMode);
            RefreshCommand = new RelayCommand(LoadClients, () => IsAuthenticated);

            AddInteractionCommand = new RelayCommand(AddInteraction, () => IsAuthenticated && SelectedClient != null);
            AddTaskCommand = new RelayCommand(AddTask, () => IsAuthenticated && SelectedClient != null);
            CompleteTaskCommand = new RelayParameterCommand(CompleteTask);
            DeleteTaskCommand = new RelayParameterCommand(DeleteTask);
            AddNoteCommand = new RelayCommand(AddNote, () => IsAuthenticated && SelectedClient != null);
            DeleteNoteCommand = new RelayParameterCommand(DeleteNote);

            ShowBirthdaysCommand = new RelayCommand(ShowBirthdays, () => IsAuthenticated);
            SendEmailCommand = new RelayCommand(SendEmail, () => IsAuthenticated && SelectedClient != null && _emailService != null);
            SendTelegramCommand = new RelayCommand(SendTelegram, () => IsAuthenticated && SelectedClient != null && _telegramService != null);
            BackupToCloudCommand = new RelayCommand(BackupToCloud, () => IsAuthenticated);
            RestoreFromCloudCommand = new RelayCommand(RestoreFromCloud, () => IsAuthenticated);

            LoginCommand = new RelayCommand(ShowLogin);
            LogoutCommand = new RelayCommand(Logout, () => IsAuthenticated);
            ChangePasswordCommand = new RelayCommand(ShowChangePassword, () => IsAuthenticated);
            ShowUserManagementCommand = new RelayCommand(ShowUserManagement, () => IsAuthenticated && IsAdmin);

            ReassignClientCommand = new RelayCommand(ReassignClient, () => IsAuthenticated && SelectedClient != null);
            PermanentDeleteClientCommand = new RelayCommand(PermanentDeleteClient, () => IsAuthenticated && IsSuperManager && SelectedDeletedClient != null && SelectedDeletedClient.IsDeleted);
            OpenRecycleBinCommand = new RelayCommand(OpenRecycleBin, () => IsAuthenticated);
            RestoreClientCommand = new RelayCommand(RestoreClient, CanRestoreClient);
            SaveUserCommand = new RelayCommand(SaveUser, CanSaveUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            ClearUserCommand = new RelayCommand(ClearUser, CanClearUser);

            ExportCommand = new RelayCommand(Export, () => IsAuthenticated);
            ExportCardCommand = new RelayCommand<string>(ExportCard, CanExportCard);
            LoggerService.LogAction("InitializeCommands", "Основные команды инициализированы");
            InitializeChatCommands();
            LoggerService.LogAction("InitializeCommands", "Конец");
        }

        // ============================================================
        // ОБНОВЛЕНИЕ КОМАНД
        // ============================================================
        private bool CanRestoreClient()
        {
            return SelectedDeletedClient != null;
        }
        private void RefreshCommands()
        {
            // Клиенты
            (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Задачи, заметки, взаимодействия
            (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddInteractionCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Экспорт
            (ExportExcelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ExportPdfCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ShowBirthdaysCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SendEmailCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (SendTelegramCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (BackupToCloudCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (OpenRecycleBinCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Пользователи (добавить эти три строки)
            (SaveUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearUserCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Авторизация
            (ShowUserManagementCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ChangePasswordCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Корзина
            (RestoreClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PermanentDeleteClientCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Чат
            (SendChatMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ExportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ExportCardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ImportCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        private void SendEmail()
        {
            if (_emailService == null || SelectedClient == null) return;

            var subject = $"Здравствуйте, {SelectedClient.Name}!";
            var body = $@"
                        <h2>Уважаемый(ая) {SelectedClient.Name}!</h2>
                        <p>Мы рады сотрудничать с вами.</p>
                        <p>Ваша компания: {SelectedClient.Company}</p>
                        <p>С уважением,<br/>Команда CRM</p>
                    ";

            Task.Run(async () =>
            {
                var result = await _emailService.SendEmailAsync(SelectedClient.Email, subject, body, true);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(result ? "Email отправлен!" : "Ошибка отправки",
                        "Результат", MessageBoxButton.OK, result ? MessageBoxImage.Information : MessageBoxImage.Error);
                });
            });
        }
        private void SendTelegram()
        {
            if (_telegramService == null || SelectedClient == null) return;

            var message = $@"
                📢 <b>Информация о клиенте</b>

                <b>Имя:</b> {SelectedClient.Name}
                <b>Телефон:</b> {SelectedClient.Phone}
                <b>Email:</b> {SelectedClient.Email}
                <b>Компания:</b> {SelectedClient.Company ?? "Не указана"}
                <b>Статус:</b> {SelectedClient.Status}";

            Task.Run(async () =>
            {
                await _telegramService.SendMessageAsync(message);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("Сообщение отправлено в Telegram!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }
        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Predicate<T> _canExecute;

            public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
            public void Execute(object parameter) => _execute((T)parameter);
            public event EventHandler CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}