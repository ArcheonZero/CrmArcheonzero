using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CrmArcheonzero.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient? _botClient;
        private readonly string? _chatId;
        private readonly bool _isEnabled;

        public TelegramService(string botToken, string chatId)
        {
            try
            {
                _isEnabled = !string.IsNullOrEmpty(botToken) &&
                             botToken != "YOUR_BOT_TOKEN" &&
                             !string.IsNullOrEmpty(chatId) &&
                             chatId != "YOUR_CHAT_ID";

                if (_isEnabled)
                {
                    _botClient = new TelegramBotClient(botToken);
                    _chatId = chatId;
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "TelegramService.Constructor");
                _isEnabled = false;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (!_isEnabled || _botClient == null || _chatId == null) return;

            try
            {
                await _botClient.SendTextMessageAsync(_chatId, message, parseMode: ParseMode.Html);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "TelegramService.SendMessageAsync");
            }
        }

        public async Task SendClientNotification(string clientName, string action)
        {
            if (!_isEnabled) return;
            var message = $@"
                📢 <b>Новое действие с клиентом</b>
                <b>Клиент:</b> {clientName}
                <b>Действие:</b> {action}
                <b>Время:</b> {DateTime.UtcNow:dd.MM.yyyy HH:mm}";
            await SendMessageAsync(message);
        }

        public async Task SendTaskNotification(string taskTitle, string clientName, DateTime dueDate)
        {
            if (!_isEnabled) return;
            var message = $@"
                ⏰ <b>Новая задача</b>
                <b>Задача:</b> {taskTitle}
                <b>Клиент:</b> {clientName}
                <b>Срок:</b> {dueDate:dd.MM.yyyy HH:mm}";
            await SendMessageAsync(message);
        }

        public async Task SendBirthdayNotification(string clientName, string company)
        {
            if (!_isEnabled) return;
            var message = $@"
                🎂 <b>День рождения клиента!</b>
                <b>Клиент:</b> {clientName}
                <b>Компания:</b> {company ?? "Не указана"}";
            await SendMessageAsync(message);
        }
    }
}