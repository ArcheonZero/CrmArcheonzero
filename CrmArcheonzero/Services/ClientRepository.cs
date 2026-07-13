using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;

namespace CrmArcheonzero.Services
{
    public class ClientRepository
    {
        private readonly IDbContext _context;

        public ClientRepository()
        {
            _context = DbContextFactory.GetDbContext();
        }

        public ClientRepository(IDbContext context)
        {
            _context = context;
        }

        // ===== ОСНОВНЫЕ МЕТОДЫ =====

        public List<Client> GetAll(bool includeDeleted = false)
        {
            var query = ((DbContext)_context).Set<Client>()
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return query.ToList();
        }
        public List<Client> GetDeleted()
        {
            return _context.Clients
                .Where(c => c.IsDeleted)
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .ToList();
        }
        public async Task<List<Client>> GetAllAsync(bool includeDeleted = false)
        {
            var query = ((DbContext)_context).Set<Client>()
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return await query.ToListAsync();
        }

        public Client? GetById(int id, bool includeDeleted = false)
        {
            var query = ((DbContext)_context).Set<Client>()
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return query.FirstOrDefault(c => c.Id == id);
        }

        public void Add(Client client)
        {
            ((DbContext)_context).Set<Client>().Add(client);
            _context.SaveChanges();
        }

        public void Update(Client client)
        {
            ((DbContext)_context).Entry(client).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DetachClient(Client client)
        {
            var context = (DbContext)_context;
            context.Entry(client).State = EntityState.Detached;
        }

        // ===== КОРЗИНА =====

        public void SoftDelete(int id, int deletedByUserId)
        {
            var client = GetById(id, true);
            if (client == null) return;

            client.IsDeleted = true;
            client.DeletedAt = DateTime.Now;
            client.DeletedByUserId = deletedByUserId;

            ((DbContext)_context).Entry(client).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Restore(int id)
        {
            var client = GetById(id, true);
            if (client == null || !client.IsDeleted) return;

            client.IsDeleted = false;
            client.DeletedAt = null;
            client.DeletedByUserId = null;

            ((DbContext)_context).Entry(client).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void PermanentDelete(int id)
        {
            var client = GetById(id, true);
            if (client == null) return;

            ((DbContext)_context).Set<Client>().Remove(client);
            _context.SaveChanges();
        }



        public List<User> GetAllUsers()
        {
            return ((DbContext)_context).Set<User>().ToList();
        }

        // ===== ПОИСК =====

        public List<Client> Search(string query, bool includeDeleted = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll(includeDeleted);

            var q = query.ToLower();
            var dbQuery = ((DbContext)_context).Set<Client>()
                .Where(c => c.Name.ToLower().Contains(q) ||
                            c.Phone.ToLower().Contains(q) ||
                            c.Email.ToLower().Contains(q) ||
                            c.Company.ToLower().Contains(q))
                .AsQueryable();

            if (!includeDeleted)
                dbQuery = dbQuery.Where(c => !c.IsDeleted);

            return dbQuery.ToList();
        }

        // ===== СТАТИСТИКА =====

        public Dictionary<string, int> GetStatistics(bool includeDeleted = false)
        {
            var context = (DbContext)_context;
            var query = context.Set<Client>().AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            var total = query.Count();
            var active = query.Count(c => c.Status == "Active");
            var inactive = query.Count(c => c.Status == "Inactive");
            var lead = query.Count(c => c.Status == "Lead");

            return new Dictionary<string, int>
            {
                ["Total"] = total,
                ["Active"] = active,
                ["Inactive"] = inactive,
                ["Lead"] = lead
            };
        }

        public bool ClientExists(string email, int excludeId = 0)
        {
            var context = (DbContext)_context;
            return context.Set<Client>().Any(c => c.Email == email && c.Id != excludeId && !c.IsDeleted);
        }

        // ===== ЗАДАЧИ, ЗАМЕТКИ, ВЗАИМОДЕЙСТВИЯ =====

        public void AddInteraction(Interaction interaction)
        {
            ((DbContext)_context).Set<Interaction>().Add(interaction);
            _context.SaveChanges();
        }

        public List<ClientTask> GetTasksByClient(int clientId)
        {
            return ((DbContext)_context).Set<ClientTask>()
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.DueDate)
                .ToList();
        }

        public void AddTask(ClientTask task)
        {
            ((DbContext)_context).Set<ClientTask>().Add(task);
            _context.SaveChanges();
        }

        public void UpdateTask(ClientTask task)
        {
            ((DbContext)_context).Entry(task).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteTask(int id)
        {
            var task = ((DbContext)_context).Set<ClientTask>().Find(id);
            if (task != null)
            {
                ((DbContext)_context).Set<ClientTask>().Remove(task);
                _context.SaveChanges();
            }
        }

        public void AddNote(Note note)
        {
            ((DbContext)_context).Set<Note>().Add(note);
            _context.SaveChanges();
        }

        public void DeleteNote(int id)
        {
            var note = ((DbContext)_context).Set<Note>().Find(id);
            if (note != null)
            {
                ((DbContext)_context).Set<Note>().Remove(note);
                _context.SaveChanges();
            }
        }

        public List<Client> GetClientsWithBirthdayInMonth(int month)
        {
            return ((DbContext)_context).Set<Client>()
                .Where(c => c.Birthday != null && c.Birthday.Value.Month == month && !c.IsDeleted)
                .ToList();
        }
        public void UpdateUser(User user)
        {
            var existing = _context.Users.Find(user.Id);
            if (existing == null) return;

            existing.Email = user.Email;
            existing.FullName = user.FullName;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;

            _context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return;

            // Мягкое удаление
            user.IsActive = false;
            _context.SaveChanges();
        }
    }
}