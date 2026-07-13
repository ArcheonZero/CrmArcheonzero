using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // ЧАТ
        // ============================================================

        private ObservableCollection<ChatMessage> _chatMessages = new();
        private string _newChatMessage = "";
        private readonly ChatService _chatService = new ChatService();

        public ObservableCollection<ChatMessage> ChatMessages
        {
            get => _chatMessages;
            set { _chatMessages = value; OnPropertyChanged(); }
        }

        public string NewChatMessage
        {
            get => _newChatMessage;
            set { _newChatMessage = value; OnPropertyChanged(); }
        }

        public ICommand SendChatMessageCommand { get; private set; } = null!;

        private void InitializeChatCommands()
        {
            SendChatMessageCommand = new RelayCommand(SendChatMessage, () => IsAuthenticated);
        }

        private void LoadChatMessages()
        {
            try
            {
                var messages = _chatService.GetMessages(50);
                ChatMessages = new ObservableCollection<ChatMessage>(messages);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "LoadChatMessages");
            }
        }

        private void SendChatMessage()
        {
            if (string.IsNullOrWhiteSpace(NewChatMessage)) return;

            var user = _userService.GetCurrentUser();
            if (user == null) return;

            try
            {
                _chatService.SendMessage(user.Id, user.FullName, NewChatMessage);
                NewChatMessage = "";
                LoadChatMessages();
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "SendChatMessage");
                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}