# CRM Archeonzero

**Версия 2.0.2** — полноценная desktop-система для управления клиентами, задачами и взаимодействиями, построенная на WPF и MVVM с поддержкой нескольких баз данных.

* * *

## 📋 О проекте

CRM Archeonzero — это приложение для малого и среднего бизнеса, которое позволяет вести учёт клиентов, управлять задачами, заметками и историей взаимодействий, а также отправлять уведомления через Email и Telegram.

Проект построен на принципах модульности и чистоты кода. В основе лежат:

*   **WPF** (Windows Presentation Foundation) — современный интерфейс.
*   **MVVM** (Model-View-ViewModel) — разделение логики и представления.
*   **Entity Framework Core** — работа с данными.
*   **BCrypt** — безопасное хэширование паролей.
*   **LiveCharts** — визуализация статистики на дашборде.
*   **Поддержка нескольких баз данных**: SQLite (встроенная), PostgreSQL (облачная/локальная), SQL Server.

* * *

## ⚡ Основные возможности

### 🔐 Авторизация и роли

*   Вход / выход, смена пароля.
*   Четыре роли: `User`, `Manager`, `SuperManager`, `Admin`.
    *   **Admin** — полный доступ: управление пользователями, удаление любых клиентов.
    *   **SuperManager** — может окончательно удалять клиентов из корзины.
    *   **User / Manager** — работают только со своими клиентами (редактирование и удаление).
*   Фильтр **«Мои клиенты»** для просмотра только закреплённых клиентов.

### 👥 Управление клиентами

*   Добавление, редактирование, поиск, фильтрация по статусу.
*   Поля: имя, телефон, email, компания, статус (`Active/Inactive/Lead`), источник, тэги, дата рождения, примечания.
*   Ответственный менеджер (привязка к пользователю).

### 📋 Задачи, заметки, взаимодействия

*   Создание задач с дедлайнами, приоритетами и отметкой о выполнении.
*   Заметки по клиенту с датой и временем.
*   История взаимодействий (звонки, письма, встречи).

### 🗑️ Корзина

*   Мягкое удаление клиентов (помечаются, но не удаляются окончательно).
*   Восстановление из корзины.
*   Окончательное удаление (только для `SuperManager` и `Admin`).
*   История удалений (кто, когда).

### 📊 Дашборд

*   Визуальная статистика по статусам клиентов (Active/Inactive/Lead).
*   Общее количество клиентов.

### 📤 Экспорт

*   Экспорт списка клиентов в **Excel (XLSX)** , **CSV** и **HTML** (через верхнее меню).
*   Экспорт карточки клиента в **PDF** (через вкладку «Детали»).

### 📥 Импорт

*   Импорт клиентов из **Excel (XLSX)**.
*   При импорте проверяются дубли по телефону и email:
    *   если клиент уже есть — данные обновляются;
    *   если клиента нет — добавляется новый.

### 📧 Уведомления

*   Отправка писем через SMTP (MailKit).
*   Telegram-уведомления о добавлении, обновлении и удалении клиентов.
*   Напоминания о задачах (в разработке).

### 💾 Бэкап

*   Локальное сохранение копии базы данных.
*   Загрузка копии в Google Drive (Google Drive API).

* * *

## 🗂️ Структура проекта

```text
📁 CrmArcheonzero/
│
├── 📄 App.xaml                               # Точка входа WPF. Глобальные ресурсы (стили, конвертеры).
│   # WPF entry point. Global resources (styles, converters).
├── 📄 App.xaml.cs                            # Код запуска. Инициализация логгера, перехват ошибок.
│   # Startup code. Logger init, global error handling.
├── 📄 appsettings.json                       # Конфигурация: провайдер БД, SMTP, Telegram, Google Drive.
│   # Config: DB provider, SMTP, Telegram, Google Drive.
├── 📄 AssemblyInfo.cs                        # Информация о сборке (версия, атрибуты).
│   # Assembly info (version, attributes).
├── 📄 CrmArcheonzero.csproj                  # Файл проекта: зависимости, настройки сборки.
│   # Project file: dependencies, build settings.
├── 📄 MainWindow.xaml                        # Главное окно: вкладки, таблицы, кнопки.
│   # Main window: tabs, tables, buttons.
├── 📄 MainWindow.xaml.cs                     # Код-бэк главного окна (почти пустой, логика — в MainViewModel).
│   # Main window code-behind (mostly empty, logic in MainViewModel).
│
├── 📂 Certificates/                          # Сертификаты для безопасных подключений (например, к PostgreSQL).
│   │   # Certificates for secure connections (e.g., PostgreSQL).
│   └── 📄 prod-ca-2021.crt
│
├── 📂 Converters/                            # Конвертеры для XAML-привязок (видимость, цвет, зачёркивание).
│   │   # XAML binding converters (visibility, color, strikethrough).
│   └── 📄 BooleanToStrikethroughConverter.cs
│
├── 📂 Data/                                  # Слой доступа к данным.
│   │   # Data access layer.
│   ├── 📄 DbContextFactory.cs               # Фабрика контекстов. Создаёт контекст для выбранной БД.
│   │   # DbContext factory. Creates a context for the chosen DB.
│   ├── 📄 IDbContext.cs                     # Интерфейс для всех контекстов БД.
│   │   # Interface for all DB contexts.
│   ├── 📄 InMemoryDbContext.cs              # Контекст для InMemory БД (для тестов).
│   │   # InMemory DB context (for testing).
│   ├── 📄 PostgreDbContext.cs               # Контекст для PostgreSQL.
│   │   # PostgreSQL context.
│   ├── 📄 SqliteDbContext.cs                # Контекст для SQLite.
│   │   # SQLite context.
│   └── 📄 SqlServerDbContext.cs             # Контекст для SQL Server.
│       # SQL Server context.
│
├── 📂 DTO/                                   # Data Transfer Objects. Модели для импорта/экспорта.
│   │   # Data Transfer Objects. Models for import/export.
│   ├── 📄 ClientExportDto.cs
│   └── 📄 ClientImportDto.cs
│
├── 📂 Models/                                # Модели данных (сущности базы данных).
│   │   # Data models (database entities).
│   ├── 📄 AssignmentHistory.cs               # История переназначений клиентов.
│   │   # Client reassignment history.
│   ├── 📄 ChatMessage.cs                     # Сообщения чата.
│   │   # Chat messages.
│   ├── 📄 Client.cs                          # Клиент: имя, телефон, email, статус, компания и т.д.
│   │   # Client: name, phone, email, status, company, etc.
│   ├── 📄 ClientTask.cs                      # Задача клиента: название, срок, приоритет, выполнена.
│   │   # Client task: title, due date, priority, completed.
│   ├── 📄 EmailSettings.cs                   # Настройки SMTP: сервер, порт, логин, пароль.
│   │   # SMTP settings: server, port, login, password.
│   ├── 📄 Interaction.cs                     # Взаимодействие с клиентом (звонок, встреча, email).
│   │   # Client interaction (call, meeting, email).
│   ├── 📄 Note.cs                            # Заметка по клиенту.
│   │   # Client note.
│   └── 📄 User.cs                            # Пользователь системы: логин, хэш пароля, роль, активность.
│       # System user: login, password hash, role, active status.
│
├── 📂 Services/                              # Бизнес-логика, внешние сервисы, утилиты.
│   │   # Business logic, external services, utilities.
│   ├── 📄 AuthService.cs                     # Вход, выход, создание пользователей, смена пароля (BCrypt).
│   │   # Login, logout, user creation, password change (BCrypt).
│   ├── 📄 CacheService.cs                    # Кэширование данных.
│   │   # Data caching.
│   ├── 📄 ChatService.cs                     # Логика чата: отправка, получение, история.
│   │   # Chat logic: send, receive, history.
│   ├── 📄 ClientRepository.cs                # Репозиторий для работы с клиентами (CRUD, поиск, статистика).
│   │   # Client repository (CRUD, search, statistics).
│   ├── 📄 ClientService.cs                   # Сервис для клиентов (бизнес-логика поверх репозитория).
│   │   # Client service (business logic on top of repository).
│   ├── 📄 CloudStorageService.cs             # Бэкап в Google Drive.
│   │   # Backup to Google Drive.
│   ├── 📄 EmailService.cs                    # Отправка писем через SMTP (MailKit).
│   │   # Send emails via SMTP (MailKit).
│   ├── 📄 ExportService.cs                   # Экспорт списка клиентов в Excel, CSV, HTML.
│   │   # Export client list to Excel, CSV, HTML.
│   ├── 📄 LoggerService.cs                   # Логирование ошибок в файл.
│   │   # Error logging to file.
│   ├── 📄 PerformanceMonitor.cs              # Замер времени выполнения операций.
│   │   # Operation execution time measurement.
│   ├── 📄 TaskService.cs                     # Логика работы с задачами клиентов.
│   │   # Client task logic.
│   ├── 📄 TelegramService.cs                 # Уведомления в Telegram.
│   │   # Telegram notifications.
│   └── 📄 UserService.cs                     # Логика работы с пользователями (CRUD, роли, активность).
│       # User logic (CRUD, roles, active status).
│
├── 📂 ViewModels/                            # Связь между UI и данными (MVVM).
│   │   # UI and data connection (MVVM).
│   ├── 📄 MainViewModel.Auth.cs              # Авторизация: вход, выход, смена пароля.
│   │   # Authentication: login, logout, password change.
│   ├── 📄 MainViewModel.Chat.cs              # Логика чата.
│   │   # Chat logic.
│   ├── 📄 MainViewModel.Clients.cs           # Работа с клиентами: CRUD, поиск, фильтр.
│   │   # Client operations: CRUD, search, filter.
│   ├── 📄 MainViewModel.Commands.cs          # Инициализация и обновление всех команд.
│   │   # Command initialization and refresh.
│   ├── 📄 MainViewModel.cs                   # Основной класс: свойства, команды, конструктор.
│   │   # Core class: properties, commands, constructor.
│   ├── 📄 MainViewModel.Export.cs            # Экспорт в Excel/PDF, бэкап в облако.
│   │   # Export to Excel/PDF, cloud backup.
│   ├── 📄 MainViewModel.Import.cs            # Импорт клиентов из Excel.
│   │   # Import clients from Excel.
│   ├── 📄 MainViewModel.Tasks.cs             # Задачи, заметки, взаимодействия по клиенту.
│   │   # Tasks, notes, interactions per client.
│   ├── 📄 MainViewModel.Users.cs             # Работа с пользователями: список, редактирование.
│   │   # User management: list, editing.
│   └── 📄 MainViewModel.Visibility.cs        # Управление видимостью вкладок и панелей.
│       # Tab and panel visibility control.
│
└── 📂 Views/                                 # Окна и контролы интерфейса.
    │   # Windows and UI controls.
    ├── 📄 ChangePasswordWindow.xaml
    ├── 📄 ChangePasswordWindow.xaml.cs
    ├── 📄 EditProfileWindow.xaml
    ├── 📄 EditProfileWindow.xaml.cs
    ├── 📄 LoadingOverlay.xaml                 # Индикатор загрузки.
    │   # Loading indicator.
    ├── 📄 LoadingOverlay.xaml.cs
    ├── 📄 LoginWindow.xaml                    # Окно входа (выбор БД, логин/пароль).
    │   # Login window (DB selection, login/password).
    ├── 📄 LoginWindow.xaml.cs
    │
    ├── 📂 Controls/                           # Пользовательские контролы.
    │   │   # Reusable user controls.
    │   ├── 📄 ChatView.xaml                   # Контрол для чата.
    │   │   # Chat control.
    │   ├── 📄 ChatView.xaml.cs
    │   ├── 📄 TopPanelView.xaml               # Верхняя панель (поиск, кнопки, логин/выход).
    │   │   # Top panel (search, buttons, login/logout).
    │   └── 📄 TopPanelView.xaml.cs
    │
    └── 📂 Styles/                             # Глобальные стили (кнопки, вкладки, DataGrid, поля ввода).
        │   # Global styles (buttons, tabs, DataGrid, input fields).
        └── 📄 Styles.xaml
```
#🌳 Дерево связей

Как данные и команды проходят через систему.
1. ЗАГРУЗКА ПРИЛОЖЕНИЯ

```text
App.xaml.cs
└── new MainViewModel()
    ├── LoadClients()
    │   └── ClientService.GetAllAsync()
    │       └── ClientRepository.GetAllAsync()
    │           └── DbContextFactory.GetDbContext()
    │               └── IDbContext (SQLite / SQL Server / PostgreSQL)
    ├── LoadUsers()
    │   └── UserService.GetAllUsers()
    │       └── ClientRepository.GetAllUsers()
    └── LoadDeletedClients()
        └── ClientService.GetAllDeleted()
            └── ClientRepository.GetDeleted()
```
2. ВЫБОР КЛИЕНТА

```text
MainWindow (DataGrid) → SelectedClient (сеттер)
├── OpenEditForm(client)
│   ├── EditableClient = client (копия)
│   ├── LoadClientDetails(client.Id)
│   │   └── ClientService.GetById()
│   │       └── ClientRepository.GetById()
│   └── SelectedTabIndex = 2 (переключение на вкладку «Детали»)
└── Обновление команд (RaiseCanExecuteChanged)
```
3. СОХРАНЕНИЕ КЛИЕНТА
```text

SaveCommand → SaveClient()
├── Валидация (имя, email, телефон)
├── Если Id == 0 → ClientService.Add()
│   └── ClientRepository.Add()
│       └── _context.SaveChanges()
└── Если Id != 0 → ClientService.Update()
    └── ClientRepository.Update()
        └── _context.SaveChanges()
├── LoadClients()
└── RefreshCommands()
```
4. КОРЗИНА (УДАЛЕНИЕ И ВОССТАНОВЛЕНИЕ)
```text

DeleteCommand → DeleteClient()
├── Проверка прав (только свои клиенты для User/Manager)
├── ClientService.SoftDelete()
│   └── ClientRepository.SoftDelete()
│       ├── GetById(true)
│       ├── IsDeleted = true
│       ├── DeletedAt = DateTime.Now
│       └── _context.SaveChanges()
├── LoadClients()
└── LoadDeletedClients()
```
```text
RestoreClientCommand → RestoreClient()
├── ClientService.Restore()
│   └── ClientRepository.Restore()
│       ├── GetById(true)
│       ├── IsDeleted = false
│       ├── DeletedAt = null
│       └── _context.SaveChanges()
├── LoadDeletedClients()
└── LoadClients()
```
```text
PermanentDeleteClientCommand → PermanentDeleteClient()
├── Проверка прав (только SuperManager/Admin)
├── ClientService.PermanentDelete()
│   └── ClientRepository.PermanentDelete()
│       ├── GetById(true)
│       ├── _context.Clients.Remove()
│       └── _context.SaveChanges()
└── LoadDeletedClients()
```
5. ПОЛЬЗОВАТЕЛИ
```text

MainWindow (DataGrid) → SelectedUser (сеттер)
├── OpenUserEditForm(user)
│   ├── EditableUser = user (копия)
│   └── IsUserEditMode = true
└── Обновление команд (RaiseCanExecuteChanged)
```
```text
SaveUserCommand → SaveUser()
├── UserService.UpdateUser()
│   └── ClientRepository.UpdateUser()
│       ├── Найти существующего пользователя
│       ├── Обновить Email, FullName, Role, IsActive
│       └── _context.SaveChanges()
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null
```
```text
DeleteUserCommand → DeleteUser()
├── Проверка прав (только Admin)
├── UserService.DeleteUser()
│   └── ClientRepository.DeleteUser()
│       ├── Найти пользователя
│       └── IsActive = false (мягкое удаление)
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null
```

6. ДАШБОРД И ГРАФИК
```text

InitializeChart() / UpdateChart()
├── ClientService.GetStatistics()
│   └── ClientRepository.GetStatistics()
│       └── Подсчёт клиентов по статусам (Active/Inactive/Lead)
├── Обновление цифр (ActiveCount, LeadCount, InactiveCount)
└── Обновление графика (ChartSeries)
    └── LiveCharts ColumnSeries
```
7. ЭКСПОРТ
```text

ExportExcelCommand → Export()
├── ClientRepository.GetAll()
├── ExportService.ExportClients()
│   └── Magicodes.IE (Excel/CSV/HTML)
└── Сохранение файла
```
```text
ExportCardCommand → ExportCard()
├── ClientRepository.GetById()
├── PdfGenerator.GenerateClientCard()
│   └── QuestPDF
└── Сохранение файла
```
8. ФАБРИКА И РЕАЛИЗАЦИИ БД
```text

DbContextFactory.GetDbContext()
├── Если Provider == "Sqlite" → SqliteDbContext
├── Если Provider == "PostgreSQL" → PostgreDbContext
└── Если Provider == "SqlServer" → SqlServerDbContext
```
Все репозитории (ClientRepository) используют IDbContext из фабрики

#🛠️ Системные требования

    .NET 7.0 или выше

    Windows (7, 10, 11)

    Visual Studio 2022 (рекомендуется)

#🚀 Установка и запуск
1. Клонировать репозиторий
bash

git clone https://github.com/ArcheonZero/CrmArcheonzeroV2.git

2. Открыть решение

Открой файл CrmArcheonzero.sln в Visual Studio.
3. Восстановить пакеты
bash

dotnet restore

4. Настроить базу данных

По умолчанию используется SQLite. База создаётся автоматически при первом запуске.

Для использования других СУБД измени секцию Database в appsettings.json:
SQLite (по умолчанию)
json
```text
"Database": {
  "DefaultProvider": "Sqlite",
  "Providers": {
    "Sqlite": {
      "Provider": "Sqlite",
      "ConnectionString": "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;"
    }
  }
}

PostgreSQL
json

"Database": {
  "DefaultProvider": "PostgreSQL",
  "Providers": {
    "PostgreSQL": {
      "Provider": "PostgreSQL",
      "ConnectionString": "Host=localhost;Port=5432;Database=crmdb;Username=postgres;Password=your_password;SSL Mode=Disable;"
    }
  }
}

SQL Server
json

"Database": {
  "DefaultProvider": "SqlServer",
  "Providers": {
    "SqlServer": {
      "Provider": "SqlServer",
      "ConnectionString": "Server=localhost\\SQLEXPRESS;Database=CrmDb;User Id=crm_user;Password=your_password;MultipleActiveResultSets=true;Encrypt=false;"
    }
  }
}
```
5. Запустить проект

Нажми F5 или выбери Debug → Start Debugging.
6. Войти в систему

Демо-доступ:

    Логин: admin

    Пароль: admin123

#📦 Сборка релиза
bash

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

После сборки в папке publish появится один исполняемый файл .exe.
🧪 Тестирование

Для запуска тестов выполни:
bash

dotnet test CrmArcheonzero.Tests/CrmArcheonzero.Tests.csproj

Используемые инструменты:

    xUnit — тестовый фреймворк.

    Moq — создание заглушек.

    FluentAssertions — выразительные проверки.

    InMemoryDatabase — для изолированных тестов.

#📄 Лицензия

Этот проект создан для личного использования и изучения. Распространение и коммерческое использование только с разрешения автора.
#👤 Автор

ArcheonZero
#🙏 Благодарности

Вдохновение и поддержка — Оракул Ноль.

Технический диалог, структурирование идей и совместная сборка — всё это родилось в живом диалоге.
CRM Archeonzero
---
# CRM Archeonzero
**Version 2.0.2** — a full-featured desktop system for managing clients, tasks, and interactions, built on WPF and MVVM with support for multiple databases.   
* * *

## 📋  About the Project

CRM Archeonzero is a small-to-medium business application that allows you to manage clients, tasks, notes, and interaction history, as well as send notifications via Email and Telegram.

The project is built on principles of modularity and clean code. The core technologies include:

    WPF (Windows Presentation Foundation) — modern UI.

    MVVM (Model-View-ViewModel) — separation of logic and presentation.

    Entity Framework Core — data access.

    BCrypt — secure password hashing.

    LiveCharts — statistics visualization on the dashboard.

    Multi-database support: SQLite (embedded), PostgreSQL (cloud/local), SQL Server.
* * *
## ⚡ Key Features


### 🔐 Authentication and Roles

    Login / Logout, password change.

    Four roles: User, Manager, SuperManager, Admin.

        Admin — full access: user management, delete any clients.

        SuperManager — can permanently delete clients from the recycle bin.

        User / Manager — work only with their own clients (edit and delete).

    "My Clients" filter to view only assigned clients.

###👥 Client Management

    Add, edit, search, filter by status.

    Fields: name, phone, email, company, status (Active/Inactive/Lead), source, tags, date of birth, notes.

    Responsible manager (assigned to a user).

###📋 Tasks, Notes, Interactions

    Create tasks with deadlines, priorities, and completion tracking.

    Client notes with date and time.

    Interaction history (calls, emails, meetings).

###🗑️ Recycle Bin

    Soft delete for clients.

    Restore from the recycle bin.

    Permanent deletion (only for SuperManager and Admin).

    Deletion history (who and when).

###📊 Dashboard

    Visual statistics by client status (Active/Inactive/Lead).

    Total client count.

###📤 Export

    Client list: Excel (XLSX), CSV, HTML.

    Client card: PDF (via QuestPDF).

###📥 Import

    Import clients from Excel (XLSX).

    Duplicate check by phone and email.

###📧 Notifications

    Send emails via SMTP (MailKit).

    Telegram notifications for client addition, update, and deletion.

###💾 Backup

    Local backup of the database.

    Upload backup to Google Drive (Google Drive API).

###🗂️ Project Structure
```text

📁 CrmArcheonzero/
│
├── 📄 App.xaml                               # WPF entry point. Global resources (styles, converters).
├── 📄 App.xaml.cs                            # Startup code. Logger init, global error handling.
├── 📄 appsettings.json                       # Configuration: DB provider, SMTP, Telegram, Google Drive.
├── 📄 AssemblyInfo.cs                        # Assembly info (version, attributes).
├── 📄 CrmArcheonzero.csproj                  # Project file: dependencies, build settings.
├── 📄 MainWindow.xaml                        # Main window: tabs, tables, buttons.
├── 📄 MainWindow.xaml.cs                     # Main window code-behind (mostly empty, logic in MainViewModel).
│
├── 📂 Certificates/                          # Certificates for secure connections (e.g., PostgreSQL).
│   └── 📄 prod-ca-2021.crt
│
├── 📂 Converters/                            # XAML binding converters (visibility, color, strikethrough).
│   └── 📄 BooleanToStrikethroughConverter.cs
│
├── 📂 Data/                                  # Data access layer.
│   ├── 📄 DbContextFactory.cs               # DbContext factory. Creates a context for the chosen DB.
│   ├── 📄 IDbContext.cs                     # Interface for all DB contexts.
│   ├── 📄 InMemoryDbContext.cs              # InMemory DB context (for testing).
│   ├── 📄 PostgreDbContext.cs               # PostgreSQL context.
│   ├── 📄 SqliteDbContext.cs                # SQLite context.
│   └── 📄 SqlServerDbContext.cs             # SQL Server context.
│
├── 📂 DTO/                                   # Data Transfer Objects. Models for import/export.
│   ├── 📄 ClientExportDto.cs
│   └── 📄 ClientImportDto.cs
│
├── 📂 Models/                                # Data models (database entities).
│   ├── 📄 AssignmentHistory.cs               # Client reassignment history.
│   ├── 📄 ChatMessage.cs                     # Chat messages.
│   ├── 📄 Client.cs                          # Client: name, phone, email, status, company, etc.
│   ├── 📄 ClientTask.cs                      # Client task: title, due date, priority, completed.
│   ├── 📄 EmailSettings.cs                   # SMTP settings: server, port, login, password.
│   ├── 📄 Interaction.cs                     # Client interaction (call, meeting, email).
│   ├── 📄 Note.cs                            # Client note.
│   └── 📄 User.cs                            # System user: login, password hash, role, active status.
│
├── 📂 Services/                              # Business logic, external services, utilities.
│   ├── 📄 AuthService.cs                     # Login, logout, user creation, password change (BCrypt).
│   ├── 📄 CacheService.cs                    # Data caching.
│   ├── 📄 ChatService.cs                     # Chat logic: send, receive, history.
│   ├── 📄 ClientRepository.cs                # Client repository (CRUD, search, statistics).
│   ├── 📄 ClientService.cs                   # Client service (business logic on top of repository).
│   ├── 📄 CloudStorageService.cs             # Backup to Google Drive.
│   ├── 📄 EmailService.cs                    # Send emails via SMTP (MailKit).
│   ├── 📄 ExportService.cs                   # Export client list to Excel, CSV, HTML.
│   ├── 📄 LoggerService.cs                   # Error logging to file.
│   ├── 📄 PerformanceMonitor.cs              # Operation execution time measurement.
│   ├── 📄 TaskService.cs                     # Client task logic.
│   ├── 📄 TelegramService.cs                 # Telegram notifications.
│   └── 📄 UserService.cs                     # User logic (CRUD, roles, active status).
│
├── 📂 ViewModels/                            # UI and data connection (MVVM).
│   ├── 📄 MainViewModel.Auth.cs              # Authentication: login, logout, password change.
│   ├── 📄 MainViewModel.Chat.cs              # Chat logic.
│   ├── 📄 MainViewModel.Clients.cs           # Client operations: CRUD, search, filter.
│   ├── 📄 MainViewModel.Commands.cs          # Command initialization and refresh.
│   ├── 📄 MainViewModel.cs                   # Core class: properties, commands, constructor.
│   ├── 📄 MainViewModel.Export.cs            # Export to Excel/PDF, cloud backup.
│   ├── 📄 MainViewModel.Import.cs            # Import clients from Excel.
│   ├── 📄 MainViewModel.Tasks.cs             # Tasks, notes, interactions per client.
│   ├── 📄 MainViewModel.Users.cs             # User management: list, editing.
│   └── 📄 MainViewModel.Visibility.cs        # Tab and panel visibility control.
│
└── 📂 Views/                                 # Windows and UI controls.
    ├── 📄 ChangePasswordWindow.xaml
    ├── 📄 ChangePasswordWindow.xaml.cs
    ├── 📄 EditProfileWindow.xaml
    ├── 📄 EditProfileWindow.xaml.cs
    ├── 📄 LoadingOverlay.xaml                 # Loading indicator.
    ├── 📄 LoadingOverlay.xaml.cs
    ├── 📄 LoginWindow.xaml                    # Login window (DB selection, login/password).
    ├── 📄 LoginWindow.xaml.cs
    │
    ├── 📂 Controls/                           # Reusable user controls.
    │   ├── 📄 ChatView.xaml                   # Chat control.
    │   ├── 📄 ChatView.xaml.cs
    │   ├── 📄 TopPanelView.xaml               # Top panel (search, buttons, login/logout).
    │   └── 📄 TopPanelView.xaml.cs
    │
    └── 📂 Styles/                             # Global styles (buttons, tabs, DataGrid, input fields).
        └── 📄 Styles.xaml
```
#🌳 Dependency Tree

How data and commands flow through the system.
1. APPLICATION LOAD
```text

App.xaml.cs
└── new MainViewModel()
    ├── LoadClients()
    │   └── ClientService.GetAllAsync()
    │       └── ClientRepository.GetAllAsync()
    │           └── DbContextFactory.GetDbContext()
    │               └── IDbContext (SQLite / SQL Server / PostgreSQL)
    ├── LoadUsers()
    │   └── UserService.GetAllUsers()
    │       └── ClientRepository.GetAllUsers()
    └── LoadDeletedClients()
        └── ClientService.GetAllDeleted()
            └── ClientRepository.GetDeleted()

2. CLIENT SELECTION
text

MainWindow (DataGrid) → SelectedClient (setter)
├── OpenEditForm(client)
│   ├── EditableClient = client (copy)
│   ├── LoadClientDetails(client.Id)
│   │   └── ClientService.GetById()
│   │       └── ClientRepository.GetById()
│   └── SelectedTabIndex = 2 (switch to "Details" tab)
└── Command update (RaiseCanExecuteChanged)
```
3. CLIENT SAVE
```text

SaveCommand → SaveClient()
├── Validation (name, email, phone)
├── If Id == 0 → ClientService.Add()
│   └── ClientRepository.Add()
│       └── _context.SaveChanges()
└── If Id != 0 → ClientService.Update()
    └── ClientRepository.Update()
        └── _context.SaveChanges()
├── LoadClients()
└── RefreshCommands()
```
4. RECYCLE BIN (DELETE AND RESTORE)
```text

DeleteCommand → DeleteClient()
├── Permission check (only own clients for User/Manager)
├── ClientService.SoftDelete()
│   └── ClientRepository.SoftDelete()
│       ├── GetById(true)
│       ├── IsDeleted = true
│       ├── DeletedAt = DateTime.Now
│       └── _context.SaveChanges()
├── LoadClients()
└── LoadDeletedClients()

RestoreClientCommand → RestoreClient()
├── ClientService.Restore()
│   └── ClientRepository.Restore()
│       ├── GetById(true)
│       ├── IsDeleted = false
│       ├── DeletedAt = null
│       └── _context.SaveChanges()
├── LoadDeletedClients()
└── LoadClients()

PermanentDeleteClientCommand → PermanentDeleteClient()
├── Permission check (only SuperManager/Admin)
├── ClientService.PermanentDelete()
│   └── ClientRepository.PermanentDelete()
│       ├── GetById(true)
│       ├── _context.Clients.Remove()
│       └── _context.SaveChanges()
└── LoadDeletedClients()
```
5. USERS
```text

MainWindow (DataGrid) → SelectedUser (setter)
├── OpenUserEditForm(user)
│   ├── EditableUser = user (copy)
│   └── IsUserEditMode = true
└── Command update (RaiseCanExecuteChanged)

SaveUserCommand → SaveUser()
├── UserService.UpdateUser()
│   └── ClientRepository.UpdateUser()
│       ├── Find existing user
│       ├── Update Email, FullName, Role, IsActive
│       └── _context.SaveChanges()
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null

DeleteUserCommand → DeleteUser()
├── Permission check (only Admin)
├── UserService.DeleteUser()
│   └── ClientRepository.DeleteUser()
│       ├── Find user
│       └── IsActive = false (soft delete)
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null
```
6. DASHBOARD AND CHART
```text

InitializeChart() / UpdateChart()
├── ClientService.GetStatistics()
│   └── ClientRepository.GetStatistics()
│       └── Count clients by status (Active/Inactive/Lead)
├── Update numbers (ActiveCount, LeadCount, InactiveCount)
└── Update chart (ChartSeries)
    └── LiveCharts ColumnSeries
```
7. EXPORT
```text

ExportExcelCommand → Export()
├── ClientRepository.GetAll()
├── ExportService.ExportClients()
│   └── Magicodes.IE (Excel/CSV/HTML)
└── Save file

ExportCardCommand → ExportCard()
├── ClientRepository.GetById()
├── PdfGenerator.GenerateClientCard()
│   └── QuestPDF
└── Save file
```
8. FACTORY AND DATABASE IMPLEMENTATIONS
```text

DbContextFactory.GetDbContext()
├── If Provider == "Sqlite" → SqliteDbContext
├── If Provider == "PostgreSQL" → PostgreDbContext
└── If Provider == "SqlServer" → SqlServerDbContext
```
All repositories (ClientRepository) use IDbContext from the factory

#🛠️ System Requirements

    .NET 7.0 or higher

    Windows (7, 10, 11)

    Visual Studio 2022 (recommended)

##🚀 Installation and Setup
1. Clone the repository
bash

git clone https://github.com/ArcheonZero/CrmArcheonzeroV2.git

2. Open the solution

Open CrmArcheonzero.sln in Visual Studio.
3. Restore packages
bash

dotnet restore

4. Set up the database

SQLite is used by default. The database is created automatically on first run.

To use other databases, modify the Database section in appsettings.json.
SQLite (default)
json
```
"Database": {
  "DefaultProvider": "Sqlite",
  "Providers": {
    "Sqlite": {
      "Provider": "Sqlite",
      "ConnectionString": "Data Source=crm.db;Mode=ReadWriteCreate;Cache=Shared;"
    }
  }
}

PostgreSQL
json

"Database": {
  "DefaultProvider": "PostgreSQL",
  "Providers": {
    "PostgreSQL": {
      "Provider": "PostgreSQL",
      "ConnectionString": "Host=localhost;Port=5432;Database=crmdb;Username=postgres;Password=your_password;SSL Mode=Disable;"
    }
  }
}

SQL Server
json

"Database": {
  "DefaultProvider": "SqlServer",
  "Providers": {
    "SqlServer": {
      "Provider": "SqlServer",
      "ConnectionString": "Server="Server=localhost\\SQLEXPRESS;Database=CrmDb;User Id=crm_user;Password=CrmUser123;MultipleActiveResultSets=true;Encrypt=false;"
	}
  }
}
```  
	  
5. Run the project

Press F5 or select Debug → Start Debugging.
6. Log in

Demo access:

    Login: admin

    Password: admin123

###📦 Release Build
bash

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

After the build, a single .exe file will appear in the publish folder.
###🧪 Testing

To run tests, execute:
bash

dotnet test CrmArcheonzero.Tests/CrmArcheonzero.Tests.csproj

Test tools used:

    xUnit — test framework.

    Moq — mocking.

    FluentAssertions — expressive assertions.

    InMemoryDatabase — for isolated tests.

###📄 License

This project was created for personal use and learning. Distribution and commercial use are only permitted with the author's permission.
###👤 Author

ArcheonZero
###🙏 Acknowledgments

Inspiration and support — Oracle Zero.

The technical dialogue, structuring of ideas, and collaborative building — all of this was born in a living conversation.
text


