using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;

namespace CrmArcheonzero.Services
{
    public class ClientService
    {
        private readonly ClientRepository _repository;

        public ClientService()
        {
            _repository = new ClientRepository();
        }

        // Для тестов
        public ClientService(ClientRepository repository)
        {
            _repository = repository;
        }

        public List<Client> GetAll(bool includeDeleted = false)
        {
            return _repository.GetAll(includeDeleted);
        }

        public async Task<List<Client>> GetAllAsync(bool includeDeleted = false)
        {
            return await _repository.GetAllAsync(includeDeleted);
        }

        public Client? GetById(int id, bool includeDeleted = false)
        {
            return _repository.GetById(id, includeDeleted);
        }

        public void Add(Client client)
        {
            _repository.Add(client);
        }

        public void Update(Client client)
        {
            _repository.Update(client);
        }

        public void SoftDelete(int id, int deletedByUserId)
        {
            _repository.SoftDelete(id, deletedByUserId);
        }

        public void Restore(int id)
        {
            _repository.Restore(id);
        }

        public void PermanentDelete(int id)
        {
            _repository.PermanentDelete(id);
        }

        public List<Client> Search(string query, bool includeDeleted = false)
        {
            return _repository.Search(query, includeDeleted);
        }

        public List<Client> GetClientsWithBirthdayInMonth(int month)
        {
            return _repository.GetClientsWithBirthdayInMonth(month);
        }

        public Dictionary<string, int> GetStatistics(bool includeDeleted = false)
        {
            return _repository.GetStatistics(includeDeleted);
        }

        public bool ClientExists(string email, int excludeId = 0)
        {
            return _repository.ClientExists(email, excludeId);
        }

        public void DetachClient(Client client)
        {
            _repository.DetachClient(client);
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ UI =====

        public List<Client> ApplyFilter(List<Client> clients, string statusFilter)
        {
            if (string.IsNullOrEmpty(statusFilter) || statusFilter == "Все")
                return clients;

            return clients.Where(c => c.Status == statusFilter).ToList();
        }

        public List<Client> ApplySearch(List<Client> clients, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return clients;

            var q = searchText.ToLower();
            return clients.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(q)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(q)) ||
                (c.Email != null && c.Email.ToLower().Contains(q)) ||
                (c.Company != null && c.Company.ToLower().Contains(q))
            ).ToList();
        }
        public List<Client> GetAllDeleted()
        {
            return _repository.GetDeleted();
        }
    }
}