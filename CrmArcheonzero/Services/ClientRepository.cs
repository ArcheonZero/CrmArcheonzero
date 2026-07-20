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
        private IDbContext? _context;

        // Ленивое свойство для доступа к контексту
        private IDbContext Context
        {
            get
            {
                if (_context == null)
                    _context = DbContextFactory.GetDbContext();
                return _context;
            }
        }

        // Конструктор по умолчанию — НЕ создаёт контекст
        public ClientRepository()
        {
            // Ничего не делаем
        }

        // Конструктор с контекстом (для тестов или внедрения)
        public ClientRepository(IDbContext context)
        {
            _context = context;
        }

        // ===== ВСЕ МЕТОДЫ ИСПОЛЬЗУЮТ Context ВМЕСТО _context =====

        public List<Client> GetAll(bool includeDeleted = false)
        {
            var query = ((DbContext)Context).Set<Client>()
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
            return ((DbContext)Context).Set<Client>()
                .Where(c => c.IsDeleted)
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .ToList();
        }

        public async Task<List<Client>> GetAllAsync(bool includeDeleted = false)
        {
            var query = ((DbContext)Context).Set<Client>()
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
            var query = ((DbContext)Context).Set<Client>()
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
            ((DbContext)Context).Set<Client>().Add(client);
            Context.SaveChanges();
        }

        public void Update(Client client)
        {
            var dbContext = (DbContext)Context;
            dbContext.Entry(client).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void DetachClient(Client client)
        {
            var context = (DbContext)Context;
            context.Entry(client).State = EntityState.Detached;
        }

        // ===== КОРЗИНА =====

        public void SoftDelete(int id, int deletedByUserId)
        {
            var client = GetById(id, true);
            if (client == null) return;

            client.IsDeleted = true;
            client.DeletedAt = DateTime.UtcNow;
            client.DeletedByUserId = deletedByUserId;

            var dbContext = (DbContext)Context;
            dbContext.Entry(client).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void Restore(int id)
        {
            var client = GetById(id, true);
            if (client == null || !client.IsDeleted) return;

            client.IsDeleted = false;
            client.DeletedAt = null;
            client.DeletedByUserId = null;

            var dbContext = (DbContext)Context;
            dbContext.Entry(client).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void PermanentDelete(int id)
        {
            var client = GetById(id, true);
            if (client == null) return;

            ((DbContext)Context).Set<Client>().Remove(client);
            Context.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return ((DbContext)Context).Set<User>().ToList();
        }

        // ===== ПОИСК =====

        public List<Client> Search(string query, bool includeDeleted = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll(includeDeleted);

            var q = query.ToLower();
            var dbQuery = ((DbContext)Context).Set<Client>()
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
            var context = (DbContext)Context;
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
            var context = (DbContext)Context;
            return context.Set<Client>().Any(c => c.Email == email && c.Id != excludeId && !c.IsDeleted);
        }

        // ===== ЗАДАЧИ, ЗАМЕТКИ, ВЗАИМОДЕЙСТВИЯ =====

        public void AddInteraction(Interaction interaction)
        {
            ((DbContext)Context).Set<Interaction>().Add(interaction);
            Context.SaveChanges();
        }

        public List<ClientTask> GetTasksByClient(int clientId)
        {
            return ((DbContext)Context).Set<ClientTask>()
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.DueDate)
                .ToList();
        }

        public void AddTask(ClientTask task)
        {
            ((DbContext)Context).Set<ClientTask>().Add(task);
            Context.SaveChanges();
        }

        public void UpdateTask(ClientTask task)
        {
            var dbContext = (DbContext)Context;
            dbContext.Entry(task).State = EntityState.Modified;
            Context.SaveChanges();
        }

        public void DeleteTask(int id)
        {
            var task = ((DbContext)Context).Set<ClientTask>().Find(id);
            if (task != null)
            {
                ((DbContext)Context).Set<ClientTask>().Remove(task);
                Context.SaveChanges();
            }
        }

        public void AddNote(Note note)
        {
            ((DbContext)Context).Set<Note>().Add(note);
            Context.SaveChanges();
        }

        public void DeleteNote(int id)
        {
            var note = ((DbContext)Context).Set<Note>().Find(id);
            if (note != null)
            {
                ((DbContext)Context).Set<Note>().Remove(note);
                Context.SaveChanges();
            }
        }

        public List<Client> GetClientsWithBirthdayInMonth(int month)
        {
            return ((DbContext)Context).Set<Client>()
                .Where(c => c.Birthday != null && c.Birthday.Value.Month == month && !c.IsDeleted)
                .ToList();
        }

        public void UpdateUser(User user)
        {
            var existing = Context.Users.Find(user.Id);
            if (existing == null) return;

            existing.Email = user.Email;
            existing.FullName = user.FullName;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;

            Context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            var user = Context.Users.Find(userId);
            if (user == null) return;

            user.IsActive = false;
            Context.SaveChanges();
        }
    }
}