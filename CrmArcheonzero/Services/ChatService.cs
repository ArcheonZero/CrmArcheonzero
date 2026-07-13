using System;
using System.Collections.Generic;
using System.Linq;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;
using Microsoft.EntityFrameworkCore;

namespace CrmArcheonzero.Services
{
    public class ChatService
    {
        private readonly IDbContext _context;

        public ChatService()
        {
            _context = DbContextFactory.GetDbContext();
        }

        public void SendMessage(int userId, string userName, string message)
        {
            var msg = new ChatMessage
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();
        }

        public List<ChatMessage> GetMessages(int count = 50)
        {
            return ((DbContext)_context).Set<ChatMessage>()
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .OrderBy(m => m.SentAt)
                .ToList();
        }
    }
}