using Microsoft.EntityFrameworkCore;
using CrmArcheonzero.Models;
using System.Threading;
using System.Threading.Tasks;

namespace CrmArcheonzero.Data
{
    public interface IDbContext
    {
        DbSet<Client> Clients { get; set; }
        DbSet<Interaction> Interactions { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        DbSet<ClientTask> Tasks { get; set; }
        DbSet<Note> Notes { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<AssignmentHistory> AssignmentHistories { get; set; } // НОВОЕ

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        void EnsureDatabaseCreated();
        void EnsureSeedData();
    }
}