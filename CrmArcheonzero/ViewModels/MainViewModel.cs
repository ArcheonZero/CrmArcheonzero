using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.ViewModels
{
    /// <summary>
    /// Основной класс ViewModel для CRM.
    /// Логика разбита по partial-файлам:
    /// - Clients: CRUD, поиск, фильтр, статистика, корзина
    /// - Tasks: задачи, заметки, взаимодействия
    /// - Auth: вход, выход, смена пароля
    /// - Visibility: управление видимостью
    /// - Commands: инициализация и обновление команд
    /// - Export: экспорт Excel, PDF, бэкап
    /// - Chat: чат (если добавлен)
    /// </summary>
    public partial class MainViewModel : INotifyPropertyChanged
    {
        // ============================================================
        // СЕРВИСЫ
        // ============================================================
        private readonly ClientService _clientService;
        private readonly TaskService _taskService;
        private readonly UserService _userService;
        private readonly EmailService? _emailService;
        private readonly TelegramService? _telegramService;
        private readonly CloudStorageService? _cloudStorage;
        private readonly PdfExportService _pdfService;

        // ============================================================
        // СОСТОЯНИЕ
        // ============================================================
        private ObservableCollection<Client> _clients = new();
        private Client? _selectedClient;
        private Client _selectedDeletedClient;
        private string _searchText = "";
        private string _statusFilter = "Все";
        private bool _isEditMode;
        private bool _isAuthenticated;
        private bool _isLoading;
        private bool _hasUnsavedChanges;
        private bool _isSearching;
        private bool _isFiltering;
        private int _selectedTabIndex;

        private ObservableCollection<Interaction> _interactions = new();
        private ObservableCollection<ClientTask> _tasks = new();
        private ObservableCollection<Note> _notes = new();
        private string _newNoteText = "";
        private string _newTaskTitle = "";
        private string _newInteractionDesc = "";
        private ObservableCollection<User> _users = new();
        private ObservableCollection<Client> _deletedClients = new();
        // ============================================================
        // СВОЙСТВА
        // ============================================================

        public ObservableCollection<Client> DeletedClients
        {
            get => _deletedClients;
            set { _deletedClients = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }
        public async Task LoadAdditionalDataAsync()
        {
            await Task.CompletedTask;
        }
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient == value) return;
                _selectedClient = value;
                OnPropertyChanged();

                if (value != null)
                {
                    OpenEditForm(value);
                    SelectedTabIndex = 2;

                    (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddNoteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddInteractionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ExportPdfCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
                else
                {
                    CloseEditForm();
                }
            }
        }

        public Client EditableClient { get; set; } = new Client();

        public Client SelectedDeletedClient
        {
            get => _selectedDeletedClient;
            set
            {
                _selectedDeletedClient = value;
                OnPropertyChanged();
                (RestoreClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                RefreshCommands();
                (PermanentDeleteClientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_isSearching) return;
                _searchText = value;
                OnPropertyChanged();
                _isSearching = true;
                Search();
                _isSearching = false;
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (_isFiltering) return;
                _statusFilter = value;
                OnPropertyChanged();
                _isFiltering = true;
                ApplyFilter();
                _isFiltering = false;
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set { _hasUnsavedChanges = value; OnPropertyChanged(); }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                if (value == 1) LoadDashboard();
                if (value == 2) LoadDetails();
            }
        }

        public bool IsAdmin => _userService.IsAdmin();
        public bool IsSuperManager => _userService.IsSuperManager();
        public User? CurrentUser => _userService.GetCurrentUser();

        public ObservableCollection<string> Statuses { get; } = new() { "Все", "Active", "Inactive", "Lead" };
        public ObservableCollection<string> InteractionTypes { get; } = new() { "Call", "Email", "Meeting", "Note" };

        public ObservableCollection<Interaction> Interactions
        {
            get => _interactions;
            set { _interactions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ClientTask> Tasks
        {
            get => _tasks;
            set { _tasks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public string NewNoteText
        {
            get => _newNoteText;
            set
            {
                if (_newNoteText == value) return;
                _newNoteText = value;
                OnPropertyChanged();

            }
        }

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set { _newTaskTitle = value; OnPropertyChanged(); }
        }

        public string NewInteractionDesc
        {
            get => _newInteractionDesc;
            set { _newInteractionDesc = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public SeriesCollection? ChartSeries { get; set; }
        public string[]? ChartLabels { get; set; }
        public Func<double, string>? ChartFormatter { get; set; }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (ЗАГЛУШКИ)
        // ============================================================

        private void LoadDashboard() { }
        private void LoadDetails() { }
        private void ReassignClient()
        {
            MessageBox.Show("Функция переназначения будет реализована в следующей версии.", "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void OpenRecycleBin()
        {
            SelectedTabIndex = 4;
        }
        private void LoadDeletedClients()
        {
            var deleted = _clientService.GetAllDeleted(); // или _repository.GetDeleted()
            DeletedClients = new ObservableCollection<Client>(deleted);
            OnPropertyChanged(nameof(DeletedClients));
        }
        // ============================================================
        // КОНСТРУКТОР
        // ============================================================
        public MainViewModel()
        {
            _clientService = new ClientService();
            _taskService = new TaskService();
            _userService = new UserService();
            _pdfService = new PdfExportService();

            try
            {
                var emailSettings = new EmailSettings
                {
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    UseSsl = true,
                    SenderEmail = "your-email@gmail.com",
                    SenderPassword = "your-app-password"
                };
                _emailService = new EmailService(emailSettings);
            }
            catch { _emailService = null; }

            try
            {
                _telegramService = new TelegramService("YOUR_BOT_TOKEN", "YOUR_CHAT_ID");
            }
            catch { _telegramService = null; }

            try
            {
                _cloudStorage = new CloudStorageService("credentials.json", "FOLDER_ID");
            }
            catch { _cloudStorage = null; }

            InitializeCommands();
            InitializeChart();
            InitializeChatCommands();
            LoadChatMessages();

            IsAuthenticated = _userService.GetCurrentUser() != null;
            HasUnsavedChanges = false;
            LoadUsers();
            UpdateVisibility();
            LoadClients();
            LoadDeletedClients();
            LoadChatMessages();
        }

        // ============================================================
        // ЧАРТ (заглушка, можно перенести в другой partial-файл)
        // ============================================================

        private void InitializeChart()
        {

            try
            {
                var stats = _clientService.GetStatistics();
                ChartSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Клиенти",
                        Values = new ChartValues<int> { stats["Active"], stats["Inactive"], stats["Lead"] }
                    }
                };
                ChartLabels = new[] { "Active", "Inactive", "Lead" };
                ChartFormatter = value => value.ToString("N0");
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "InitializeChart");
            }
        }

        // ============================================================
        // INotifyPropertyChanged
        // ============================================================
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ============================================================
    // RELAY COMMAND (вынесен сюда, чтобы не создавать отдельный файл)
    // ============================================================
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayParameterCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayParameterCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}