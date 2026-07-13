using System;
using System.Collections.Generic;
using System.Linq;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;

namespace CrmArcheonzero.Services
{
    public class TaskService
    {
        private readonly ClientRepository _repository;

        public TaskService()
        {
            _repository = new ClientRepository();
        }

        public TaskService(ClientRepository repository)
        {
            _repository = repository;
        }

        public void AddTask(ClientTask task)
        {
            _repository.AddTask(task);
        }

        public void UpdateTask(ClientTask task)
        {
            _repository.UpdateTask(task);
        }

        public void DeleteTask(int id)
        {
            _repository.DeleteTask(id);
        }

        public List<ClientTask> GetTasksByClient(int clientId)
        {
            return _repository.GetTasksByClient(clientId);
        }

        public void AddNote(Note note)
        {
            _repository.AddNote(note);
        }

        public void DeleteNote(int id)
        {
            _repository.DeleteNote(id);
        }

        public void AddInteraction(Interaction interaction)
        {
            _repository.AddInteraction(interaction);
        }
    }
}