using System;
using System.Collections.ObjectModel;
using System.Windows;
using CrmArcheonzero.Models;
using CrmArcheonzero.Services;

namespace CrmArcheonzero.ViewModels
{
    public partial class MainViewModel
    {
        // ============================================================
        // ЗАДАЧИ, ЗАМЕТКИ, ВЗАИМОДЕЙСТВИЯ
        // ============================================================

        private void AddInteraction()
        {
            if (SelectedClient == null || string.IsNullOrWhiteSpace(NewInteractionDesc)) return;

            try
            {
                var interaction = new Interaction
                {
                    ClientId = SelectedClient.Id,
                    Date = DateTime.UtcNow,
                    Type = "Call",
                    Description = NewInteractionDesc,
                    Outcome = "Новое"
                };

                _taskService.AddInteraction(interaction);
                LoadClientDetails(SelectedClient.Id);
                NewInteractionDesc = "";
                HasUnsavedChanges = true;
                MessageBox.Show("Взаимодействие добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AddInteraction");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTask()
        {
            if (SelectedClient == null || string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            try
            {
                var task = new ClientTask
                {
                    ClientId = SelectedClient.Id,
                    Title = NewTaskTitle,
                    Description = "Новая задача",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Priority = "Medium",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _taskService.AddTask(task);
                LoadClientDetails(SelectedClient.Id);
                NewTaskTitle = "";
                HasUnsavedChanges = true;
                _telegramService?.SendTaskNotification(task.Title, SelectedClient.Name, task.DueDate);
                MessageBox.Show("Задача добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AddTask");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteTask(object? parameter)
        {
            if (parameter is ClientTask task)
            {
                try
                {
                    task.IsCompleted = !task.IsCompleted;
                    _taskService.UpdateTask(task);
                    LoadClientDetails(SelectedClient!.Id);
                    HasUnsavedChanges = true;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError(ex, "CompleteTask");
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteTask(object? parameter)
        {
            if (parameter is ClientTask task)
            {
                var result = MessageBox.Show("Удалить задачу?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _taskService.DeleteTask(task.Id);
                        LoadClientDetails(SelectedClient!.Id);
                        HasUnsavedChanges = true;
                    }
                    catch (Exception ex)
                    {
                        LoggerService.LogError(ex, "DeleteTask");
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddNote()
        {
            if (SelectedClient == null || string.IsNullOrWhiteSpace(NewNoteText)) return;

            try
            {
                var note = new Note
                {
                    ClientId = SelectedClient.Id,
                    Content = NewNoteText,
                    CreatedAt = DateTime.UtcNow
                };

                _taskService.AddNote(note);
                LoadClientDetails(SelectedClient.Id);
                NewNoteText = "";
                HasUnsavedChanges = true;
            }
            catch (Exception ex)
            {
                LoggerService.LogError(ex, "AddNote");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteNote(object? parameter)
        {
            if (parameter is Note note)
            {
                var result = MessageBox.Show("Удалить заметку?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _taskService.DeleteNote(note.Id);
                        LoadClientDetails(SelectedClient!.Id);
                        HasUnsavedChanges = true;
                    }
                    catch (Exception ex)
                    {
                        LoggerService.LogError(ex, "DeleteNote");
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}