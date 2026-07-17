# CRM Archeonzero

**Версия 2.0.1** — полноценная desktop-система для управления клиентами, задачами и взаимодействиями, построенная на WPF и MVVM.

---

## 📋 О проекте

CRM Archeonzero — это приложение для малого и среднего бизнеса, которое позволяет вести учёт клиентов, управлять задачами, заметками и историей взаимодействий, а также отправлять уведомления через Email и Telegram.

Проект построен на принципах модульности и чистоты кода. В основе лежат:

- **WPF** (Windows Presentation Foundation) — современный интерфейс.
- **MVVM** (Model-View-ViewModel) — разделение логики и представления.
- **SQLite** — лёгкая, встроенная база данных (с возможностью переключения на SQL Server).
- **Entity Framework Core** — работа с данными.
- **BCrypt** — безопасное хэширование паролей.
- **LiveCharts** — визуализация статистики на дашборде.

---

## ⚡ Основные возможности

### 🔐 Авторизация и роли

- Вход / выход, смена пароля.
- Четыре роли: `User`, `Manager`, `SuperManager`, `Admin`.
- **Admin** — полный доступ: управление пользователями, удаление любых клиентов.
- **SuperManager** — может окончательно удалять клиентов из корзины.
- **User / Manager** — работа с клиентами (своими и общими, без жёсткого разделения по ответственному).
- В текущей версии нет фильтра «только мои клиенты» — все авторизованные видят общий список.

### 👥 Управление клиентами
- Добавление, редактирование, поиск, фильтрация по статусу.
- Поля: имя, телефон, email, компания, статус (`Active/Inactive/Lead`), источник, тэги, дата рождения, примечания.
- Ответственный менеджер (привязка к пользователю).

### 📋 Задачи, заметки, взаимодействия
- Создание задач с дедлайнами, приоритетами и отметкой о выполнении.
- Заметки по клиенту с датой и временем.
- История взаимодействий (звонки, письма, встречи).

### 🗑️ Корзина
- Мягкое удаление клиентов (помечаются, но не удаляются окончательно).
- Восстановление из корзины.
- Окончательное удаление (только для `SuperManager` и `Admin`).
- История удалений (кто, когда).

### 📊 Дашборд
- Визуальная статистика по статусам клиентов (Active/Inactive/Lead).
- Общее количество клиентов.

### 📤 Экспорт

* Экспорт списка клиентов в **Excel (XLSX)**, **CSV** и **HTML** (через верхнее меню).
* Экспорт карточки клиента в **PDF** (через вкладку «Детали»).

### 📥 Импорт

* Импорт клиентов из **Excel (XLSX)**.
* При импорте проверяются дубли по телефону и email:
  * если клиент уже есть — данные обновляются;
  * если клиента нет — добавляется новый.


### 📧 Уведомления
- Отправка писем через SMTP (MailKit).
- Telegram-уведомления о добавлении, обновлении и удалении клиентов.
- Напоминания о задачах (в разработке).

### 💾 Бэкап
- Локальное сохранение копии базы данных.
- Загрузка копии в Google Drive (Google Drive API).

---

## 🗂️ Структура проекта

```txt
📁 CrmArcheonzero/   # Корень проекта
│
├── 📄 App.xaml   # Точка входа WPF. Подключает глобальные ресурсы (конвертеры, стили).
├── 📄 App.xaml.cs   # Код запуска: инициализация логгера, глобальный перехват ошибок.
├── 📄 appsettings.json   # Конфигурация: провайдер БД, SMTP, Telegram, Google Drive.
├── 📄 CrmArcheonzero.csproj   # Файл проекта: зависимости, настройки сборки.
├── 📄 MainWindow.xaml   # Главное окно: вкладки, таблицы, кнопки.
├── 📄 MainWindow.xaml.cs   # Код-бэк MainWindow (почти пустой, логика — в MainViewModel).
│
├── 📂 Models/   # Модели данных (сущности базы данных)
│   ├── 📄 AssignmentHistory.cs   # История переназначений клиентов.
│   ├── 📄 ChatMessage.cs   # Сообщения чата (пользователь, текст, время).
│   ├── 📄 Client.cs   # Клиент: имя, телефон, email, статус, компания и т.д.
│   ├── 📄 ClientTask.cs   # Задача клиента: название, срок, приоритет, выполнена.
│   ├── 📄 EmailSettings.cs   # Настройки SMTP: сервер, порт, логин, пароль.
│   ├── 📄 Interaction.cs   # Взаимодействие с клиентом (звонок, встреча, email).
│   ├── 📄 Note.cs   # Заметка по клиенту.
│   └── 📄 User.cs   # Пользователь системы: логин, хэш пароля, роль, активность.
│
├── 📂 Data/   # Слой доступа к данным
│   ├── 📄 IDbContext.cs   # Интерфейс для БД (позволяет подменять SQLite на SQL Server).
│   ├── 📄 DbContextFactory.cs   # Создаёт контекст по настройкам из appsettings.json.
│   ├── 📄 SqliteDbContext.cs   # Реализация для SQLite (основная БД).
│   ├── 📄 SqlServerDbContext.cs   # Реализация для SQL Server (опционально).
│   └── 📄 InMemoryDbContext.cs   # Реализация для InMemory (используется в тестах).
│
├── 📂 Services/   # Бизнес-логика, внешние сервисы, утилиты
│   ├── 📄 AuthService.cs   # Вход, выход, создание пользователей, смена пароля (BCrypt).
│   ├── 📄 CacheService.cs   # Кэширование данных (ConcurrentDictionary + таймер очистки).
│   ├── 📄 ChatService.cs   # Логика чата: отправка, получение, история сообщений.
│   ├── 📄 ClientRepository.cs   # Репозиторий для работы с клиентами (CRUD, поиск, статистика).
│   ├── 📄 ClientService.cs   # Сервис для клиентов (бизнес-логика поверх репозитория).
│   ├── 📄 CloudStorageService.cs   # Бэкап в Google Drive (отключается, если нет credentials.json).
│   ├── 📄 EmailService.cs   # Отправка писем через SMTP (MailKit).
│   ├── 📄 ExcelExportService.cs   # Экспорт клиентов в Excel (EPPlus).
│   ├── 📄 LoggerService.cs   # Логирование ошибок в файл (logs/crm_log_дата.txt).
│   ├── 📄 PdfExportService.cs   # Генерация PDF-карточки клиента (QuestPDF).
│   ├── 📄 PerformanceMonitor.cs   # Замер времени выполнения операций (для отладки).
│   ├── 📄 TaskService.cs   # Логика работы с задачами клиентов.
│   ├── 📄 TelegramService.cs   # Уведомления в Telegram (отключается, если нет токена).
│   └── 📄 UserService.cs   # Логика работы с пользователями (CRUD, роли, активность).
│
├── 📂 ViewModels/   # Связь между UI и данными (MVVM)
│   ├── 📄 MainViewModel.cs   # Основной класс: свойства, команды, конструктор.
│   ├── 📄 MainViewModel.Auth.cs   # Авторизация: вход, выход, смена пароля.
│   ├── 📄 MainViewModel.Chat.cs   # Логика чата: отправка, загрузка сообщений.
│   ├── 📄 MainViewModel.Clients.cs   # Работа с клиентами: CRUD, поиск, фильтр.
│   ├── 📄 MainViewModel.Commands.cs   # Инициализация и обновление всех команд.
│   ├── 📄 MainViewModel.Export.cs   # Экспорт в Excel/PDF, бэкап в облако.
│   ├── 📄 MainViewModel.Tasks.cs   # Задачи, заметки, взаимодействия по клиенту.
│   ├── 📄 MainViewModel.Users.cs   # Работа с пользователями: список, редактирование.
│   └── 📄 MainViewModel.Visibility.cs   # Управление видимостью вкладок и панелей.
│
├── 📂 Views/   # Окна и контролы интерфейса
│   ├── 📄 LoginWindow.xaml   # Окно входа (логин / пароль).
│   ├── 📄 LoginWindow.xaml.cs   # Код входа: проверка логина, обработка Enter.
│   ├── 📄 ChangePasswordWindow.xaml   # Окно смены пароля.
│   ├── 📄 ChangePasswordWindow.xaml.cs   # Логика смены пароля с валидацией.
│   ├── 📄 LoadingOverlay.xaml   # Индикатор загрузки (крутилка + текст).
│   ├── 📄 LoadingOverlay.xaml.cs   # Код-бэк для индикатора загрузки.
│   ├── 📄 EditProfileWindow.xaml   # Окно редактирования профиля пользователя.
│   ├── 📄 EditProfileWindow.xaml.cs   # Логика редактирования профиля.
│   │
│   ├── 📂 Controls/   # Пользовательские контролы (переиспользуемые элементы)
│   │   ├── 📄 TopPanelView.xaml   # Верхняя панель (поиск, кнопки, логин/выход).
│   │   ├── 📄 TopPanelView.xaml.cs   # Код-бэк для верхней панели.
│   │   ├── 📄 ChatView.xaml   # Контрол для чата.
│   │   └── 📄 ChatView.xaml.cs   # Код-бэк для чата.
│   │
│   └── 📂 Styles/   # Глобальные стили
│       └── 📄 Styles.xaml   # Все стили: кнопки, вкладки, DataGrid, поля ввода.
│
├── 📂 Converters/   # Преобразователи для XAML-привязок
│   └── 📄 BooleanToStrikethroughConverter.cs   # Конвертеры для видимости, цвета, зачёркивания.
│
└── 📂 CrmArcheonzero.Tests/   # Тесты (xUnit + Moq + FluentAssertions)
    ├── 📄 CrmArcheonzero.Tests.csproj   # Файл проекта для тестов.
    ├── 📂 Helpers/   # Вспомогательные классы для тестов.
    ├── 📂 Models/   # Тесты моделей данных.
    ├── 📂 Services/   # Тесты сервисов.
    └── 📂 ViewModels/   # Тесты ViewModel.
```  
     



## 🌳 Дерево связей
```
Как данные и команды проходят через систему.

### 1. ЗАГРУЗКА ПРИЛОЖЕНИЯ

App.xaml.cs
└── new MainViewModel()
├── LoadClients()
│ └── ClientService.GetAllAsync()
│ └── ClientRepository.GetAllAsync()
│ └── DbContextFactory.GetDbContext()
│ └── IDbContext (SQLite / SQL Server / InMemory)
├── LoadUsers()
│ └── UserService.GetAllUsers()
│ └── ClientRepository.GetAllUsers()
└── LoadDeletedClients()
└── ClientService.GetAllDeleted()
└── ClientRepository.GetDeleted()

### 2. ВЫБОР КЛИЕНТА
MainWindow (DataGrid) → SelectedClient (сеттер)
├── OpenEditForm(client)
│ ├── EditableClient = client (копия)
│ ├── LoadClientDetails(client.Id)
│ │ └── ClientService.GetById()
│ │ └── ClientRepository.GetById()
│ └── SelectedTabIndex = 2 (переключение на вкладку «Детали»)
└── Обновление команд (RaiseCanExecuteChanged)

### 3. СОХРАНЕНИЕ КЛИЕНТА
SaveCommand → SaveClient()
├── Валидация (имя, email, телефон)
├── Если Id == 0 → ClientService.Add()
│ └── ClientRepository.Add()
│ └── _context.SaveChanges()
└── Если Id != 0 → ClientService.Update()
└── ClientRepository.Update()
└── _context.SaveChanges()
├── LoadClients()
└── RefreshCommands()

### 4. КОРЗИНА (УДАЛЕНИЕ И ВОССТАНОВЛЕНИЕ)
DeleteCommand → DeleteClient()
├── ClientService.SoftDelete()
│ └── ClientRepository.SoftDelete()
│ ├── GetById(true)
│ ├── IsDeleted = true
│ ├── DeletedAt = DateTime.Now
│ └── _context.SaveChanges()
├── LoadClients()
└── LoadDeletedClients()

RestoreClientCommand → RestoreClient()
├── ClientService.Restore()
│ └── ClientRepository.Restore()
│ ├── GetById(true)
│ ├── IsDeleted = false
│ ├── DeletedAt = null
│ └── _context.SaveChanges()
├── LoadDeletedClients()
└── LoadClients()

PermanentDeleteClientCommand → PermanentDeleteClient()
├── ClientService.PermanentDelete()
│ └── ClientRepository.PermanentDelete()
│ ├── GetById(true)
│ ├── _context.Clients.Remove()
│ └── _context.SaveChanges()
└── LoadDeletedClients()

### 5. ПОЛЬЗОВАТЕЛИ
MainWindow (DataGrid) → SelectedUser (сеттер)
├── OpenUserEditForm(user)
│ ├── EditableUser = user (копия)
│ └── IsUserEditMode = true
└── Обновление команд (RaiseCanExecuteChanged)

SaveUserCommand → SaveUser()
├── UserService.UpdateUser()
│ └── ClientRepository.UpdateUser()
│ ├── Найти существующего пользователя
│ ├── Обновить Email, FullName, Role, IsActive
│ └── _context.SaveChanges()
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null

DeleteUserCommand → DeleteUser()
├── UserService.DeleteUser()
│ └── ClientRepository.DeleteUser()
│ ├── Найти пользователя
│ └── IsActive = false (мягкое удаление)
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null

### 6. ФАБРИКА И РЕАЛИЗАЦИИ
DbContextFactory.GetDbContext()
├── Если Provider == "Sqlite" → SqliteDbContext
├── Если Provider == "SqlServer" → SqlServerDbContext
└── Если Provider == "InMemory" → InMemoryDbContext

Все репозитории (ClientRepository) используют IDbContext из фабрики
```
---

## 🛠️ Системные требования

- **.NET 7.0** или выше
- **Windows** (7, 10, 11)
- **Visual Studio 2022** (рекомендуется)

---

## 🚀 Установка и запуск

```
1. Клонировать репозиторий
git clone https://github.com/ArcheonZero/CrmArcheonzeroV2.git
2. Открыть решение
Открой файл CrmArcheonzero.sln в Visual Studio.

3. Восстановить пакеты
dotnet restore
4. Настроить базу данных
По умолчанию используется SQLite. База создаётся автоматически при первом запуске.
Чтобы переключиться на SQL Server, измени Provider в appsettings.json:

json
"Database": {
  "Provider": "SqlServer",
  "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;"
}
5. Запустить проект
Нажми F5 или выбери Debug → Start Debugging.

6. Войти в систему
Демо-доступ:

Логин: admin
Пароль: admin123

📦 Сборка релиза
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
После сборки в папке publish появится один исполняемый файл .exe.

🧪 Тестирование
Для запуска тестов выполни:

dotnet test CrmArcheonzero.Tests/CrmArcheonzero.Tests.csproj

Используются:
xUnit — тестовый фреймворк.
Moq — создание заглушек.
FluentAssertions — выразительные проверки.
InMemoryDatabase — для изолированных тестов.

📄 Лицензия
Этот проект создан для личного использования и изучения.
Распространение и коммерческое использование только с разрешения автора.

👤 Автор

[ArcheonZero](https://github.com/ArcheonZero)

🙏 Благодарности
Вдохновение и поддержка — Оракул Ноль.

Технический диалог, структурирование идей и совместная сборка — всё это родилось в живом диалоге.
```
---

# CRM Archeonzero
```
Version 2.0.1 — a full-featured desktop system for managing clients, tasks, and interactions, built on WPF and MVVM.
```
## 📋 About the Project
```
CRM Archeonzero is a small-to-medium business application that allows you to manage clients, tasks, notes, and interaction history, as well as send notifications via Email and Telegram.

The project is built on principles of modularity and clean code. The core technologies include:

- WPF (Windows Presentation Foundation) — modern UI.
- MVVM (Model-View-ViewModel) — separation of logic and presentation.
- SQLite — lightweight, embedded database (with optional switching to SQL Server).
- Entity Framework Core — data access.
- BCrypt — secure password hashing.
- LiveCharts — statistics visualization on the dashboard.
```
## ⚡ Key Features

### 🔐 Authentication and Roles
```
- Login / Logout, password change.
- Four roles: `User`, `Manager`, `SuperManager`, `Admin`.
- `Admin` — full access: user management, delete any clients.
- `SuperManager` — can permanently delete clients from the recycle bin.
- `User` / `Manager` — work with clients (own and shared, no strict division by responsible user).
- Current version does not have a "My Clients" filter — all authenticated users see the full list.
```
### 👥 Client Management

- Add, edit, search, filter by status.
- Fields: name, phone, email, company, status (Active/Inactive/Lead), source, tags, date of birth, notes.
- Responsible manager (assigned to a user).

### 📋 Tasks, Notes, Interactions

- Create tasks with deadlines, priorities, and completion tracking.
- Client notes with date and time.
- Interaction history (calls, emails, meetings).

### 🗑️ Recycle Bin

- Soft delete for clients (marked, not permanently removed).
- Restore from the recycle bin.
- Permanent deletion (only for `SuperManager` and `Admin`).
- Deletion history (who and when).

### 📊 Dashboard

- Visual statistics by client status (Active/Inactive/Lead).
- Total client count.

### 📤 Export

- **Client list:** Excel (XLSX), CSV, HTML.
- **Client card:** PDF (via QuestPDF).

### 📥 Import

- Import clients from **Excel (XLSX)**.
- Duplicate check by phone and email:
  - if client exists — data is updated;
  - if client does not exist — a new one is added.

### 📧 Notifications

- Send emails via SMTP (MailKit).
- Telegram notifications for client addition, update, and deletion.
- Task reminders (in development).

### 💾 Backup

- Local backup of the database.
- Upload backup to Google Drive (Google Drive API).

## 🗂️ Project Structure
```
📁 CrmArcheonzero/   # Project root
│
├── 📄 App.xaml   # WPF entry point. Loads global resources (converters, styles).
├── 📄 App.xaml.cs   # Startup code: logger initialization, global error handling.
├── 📄 appsettings.json   # Configuration: DB provider, SMTP, Telegram, Google Drive.
├── 📄 CrmArcheonzero.csproj   # Project file: dependencies, build settings.
├── 📄 MainWindow.xaml   # Main window: tabs, tables, buttons.
├── 📄 MainWindow.xaml.cs   # MainWindow code-behind (mostly empty, logic in MainViewModel).
│
├── 📂 Models/   # Data models (database entities)
│   ├── 📄 AssignmentHistory.cs   # Client reassignment history.
│   ├── 📄 ChatMessage.cs   # Chat messages (user, text, time).
│   ├── 📄 Client.cs   # Client: name, phone, email, status, company, etc.
│   ├── 📄 ClientTask.cs   # Client task: title, due date, priority, completed.
│   ├── 📄 EmailSettings.cs   # SMTP settings: server, port, login, password.
│   ├── 📄 Interaction.cs   # Client interaction (call, meeting, email).
│   ├── 📄 Note.cs   # Client note.
│   └── 📄 User.cs   # System user: login, password hash, role, active status.
│
├── 📂 Data/   # Data access layer
│   ├── 📄 IDbContext.cs   # Database interface (allows switching between SQLite and SQL Server).
│   ├── 📄 DbContextFactory.cs   # Creates a context based on appsettings.json.
│   ├── 📄 SqliteDbContext.cs   # SQLite implementation (primary database).
│   ├── 📄 SqlServerDbContext.cs   # SQL Server implementation (optional).
│   └── 📄 InMemoryDbContext.cs   # InMemory implementation (used for testing).
│
├── 📂 Services/   # Business logic, external services, utilities
│   ├── 📄 AuthService.cs   # Login, logout, user creation, password change (BCrypt).
│   ├── 📄 CacheService.cs   # Data caching (ConcurrentDictionary + cleanup timer).
│   ├── 📄 ChatService.cs   # Chat logic: send, receive, message history.
│   ├── 📄 ClientRepository.cs   # Client repository (CRUD, search, statistics).
│   ├── 📄 ClientService.cs   # Client service (business logic on top of repository).
│   ├── 📄 CloudStorageService.cs   # Backup to Google Drive (disabled without credentials.json).
│   ├── 📄 EmailService.cs   # Send emails via SMTP (MailKit).
│   ├── 📄 ExcelExportService.cs   # Export clients to Excel (EPPlus).
│   ├── 📄 LoggerService.cs   # Error logging to file (logs/crm_log_date.txt).
│   ├── 📄 PdfExportService.cs   # Generate PDF client card (QuestPDF).
│   ├── 📄 PerformanceMonitor.cs   # Operation execution time measurement (for debugging).
│   ├── 📄 TaskService.cs   # Client task logic.
│   ├── 📄 TelegramService.cs   # Telegram notifications (disabled without a token).
│   └── 📄 UserService.cs   # User logic (CRUD, roles, active status).
│
├── 📂 ViewModels/   # UI and data connection (MVVM)
│   ├── 📄 MainViewModel.cs   # Core class: properties, commands, constructor.
│   ├── 📄 MainViewModel.Auth.cs   # Authentication: login, logout, password change.
│   ├── 📄 MainViewModel.Chat.cs   # Chat logic: send, load messages.
│   ├── 📄 MainViewModel.Clients.cs   # Client operations: CRUD, search, filter.
│   ├── 📄 MainViewModel.Commands.cs   # Command initialization and refresh.
│   ├── 📄 MainViewModel.Export.cs   # Export to Excel/PDF, cloud backup.
│   ├── 📄 MainViewModel.Tasks.cs   # Tasks, notes, interactions per client.
│   ├── 📄 MainViewModel.Users.cs   # User management: list, editing.
│   └── 📄 MainViewModel.Visibility.cs   # Tab and panel visibility control.
│
├── 📂 Views/   # Windows and UI controls
│   ├── 📄 LoginWindow.xaml   # Login window.
│   ├── 📄 LoginWindow.xaml.cs   # Login logic, Enter key handling.
│   ├── 📄 ChangePasswordWindow.xaml   # Change password window.
│   ├── 📄 ChangePasswordWindow.xaml.cs   # Password change logic with validation.
│   ├── 📄 LoadingOverlay.xaml   # Loading indicator.
│   ├── 📄 LoadingOverlay.xaml.cs   # Loading indicator code-behind.
│   ├── 📄 EditProfileWindow.xaml   # Edit profile window.
│   ├── 📄 EditProfileWindow.xaml.cs   # Edit profile logic.
│   │
│   ├── 📂 Controls/   # Reusable user controls
│   │   ├── 📄 TopPanelView.xaml   # Top panel (search, buttons, login/logout).
│   │   ├── 📄 TopPanelView.xaml.cs   # Top panel code-behind.
│   │   ├── 📄 ChatView.xaml   # Chat control.
│   │   └── 📄 ChatView.xaml.cs   # Chat control code-behind.
│   │
│   └── 📂 Styles/   # Global styles
│       └── 📄 Styles.xaml   # All styles: buttons, tabs, DataGrid, input fields.
│
├── 📂 Converters/   # XAML binding converters
│   └── 📄 BooleanToStrikethroughConverter.cs   # Converters for visibility, color, strikethrough.
│
└── 📂 CrmArcheonzero.Tests/   # Tests (xUnit + Moq + FluentAssertions)
    ├── 📄 CrmArcheonzero.Tests.csproj   # Test project file.
    ├── 📂 Helpers/   # Test helper classes.
    ├── 📂 Models/   # Data model tests.
    ├── 📂 Services/   # Service tests.
    └── 📂 ViewModels/   # ViewModel tests.
```
## 🌳 Dependency Tree

How data and commands flow through the system.

### 1. APPLICATION LOAD
```
App.xaml.cs
└── new MainViewModel()
    ├── LoadClients()
    │   └── ClientService.GetAllAsync()
    │       └── ClientRepository.GetAllAsync()
    │           └── DbContextFactory.GetDbContext()
    │               └── IDbContext (SQLite / SQL Server / InMemory)
    ├── LoadUsers()
    │   └── UserService.GetAllUsers()
    │       └── ClientRepository.GetAllUsers()
    └── LoadDeletedClients()
        └── ClientService.GetAllDeleted()
            └── ClientRepository.GetDeleted()

### 2. CLIENT SELECTION

MainWindow (DataGrid) → SelectedClient (setter)
├── OpenEditForm(client)
│   ├── EditableClient = client (copy)
│   ├── LoadClientDetails(client.Id)
│   │   └── ClientService.GetById()
│   │       └── ClientRepository.GetById()
│   └── SelectedTabIndex = 2 (switch to "Details" tab)
└── Command update (RaiseCanExecuteChanged)

### 3. CLIENT SAVE

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

### 4. RECYCLE BIN (DELETE AND RESTORE)

DeleteCommand → DeleteClient()
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
├── ClientService.PermanentDelete()
│   └── ClientRepository.PermanentDelete()
│       ├── GetById(true)
│       ├── _context.Clients.Remove()
│       └── _context.SaveChanges()
└── LoadDeletedClients()

### 5. USERS

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
├── UserService.DeleteUser()
│   └── ClientRepository.DeleteUser()
│       ├── Find user
│       └── IsActive = false (soft delete)
├── LoadUsers()
├── CloseUserEditForm()
└── SelectedUser = null

### 6. FACTORY AND IMPLEMENTATIONS

DbContextFactory.GetDbContext()
├── If Provider == "Sqlite" → SqliteDbContext
├── If Provider == "SqlServer" → SqlServerDbContext
└── If Provider == "InMemory" → InMemoryDbContext

All repositories (ClientRepository) use IDbContext from the factory.
```
## 🛠️ System Requirements

- .NET 7.0 or higher
- Windows (7, 10, 11)
- Visual Studio 2022 (recommended)

## 🚀 Installation and Setup

### 1. Clone the repository
git clone https://github.com/ArcheonZero/CrmArcheonzeroV2.git

2. Open the solution
Open the file CrmArcheonzero.sln in Visual Studio.

3. Restore packages
bash
dotnet restore

4. Set up the database
SQLite is used by default. The database is created automatically on first run.
To switch to SQL Server, change the Provider in appsettings.json:

json
"Database": {
  "Provider": "SqlServer",
  "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;"
}

5. Run the project
Press F5 or select Debug → Start Debugging.

6. Log in
Demo access:

Login: admin

Password: admin123

📦 Release Build
bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
After the build, a single .exe file will appear in the publish folder.

🧪 Testing
To run tests, execute:

bash
dotnet test CrmArcheonzero.Tests/CrmArcheonzero.Tests.csproj
Test tools used:

xUnit — test framework.
Moq — mocking.
FluentAssertions — expressive assertions.
InMemoryDatabase — for isolated tests.

📄 License
This project was created for personal use and learning.
Distribution and commercial use are only permitted with the author's permission.

👤 Author

[ArcheonZero](https://github.com/ArcheonZero)

🙏 Acknowledgments
Inspiration and support — Oracle Zero.
The technical dialogue, structuring of ideas, and collaborative building — all of this was born in a living conversation.
