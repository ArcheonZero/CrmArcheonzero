using CrmArcheonzero.Data;
using CrmArcheonzero.Models;
using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrmArcheonzero.Services
{
    public class UserService
    {
        private IDbContext? _context;
        private readonly ClientRepository _repository;
        private readonly AuthService _authService;
        public AuthService GetAuthService() => _authService;
        public UserService()
        {
            _repository = new ClientRepository();
            _authService = new AuthService();
        }
        private IDbContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = DbContextFactory.GetDbContext();
                    if (_context == null)
                    {
                        throw new Exception("Нет подключения к базе данных");
                    }
                }
                return _context;
            }
        }
        public UserService(ClientRepository repository, AuthService authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public List<User> GetAllUsers()
        {
            return _repository.GetAllUsers();
        }

        public bool IsAdmin()
        {
            return _authService.IsAdmin();
        }

        public bool IsSuperManager()
        {
            return _authService.IsSuperManager();
        }
        public bool IsManager()
        {
            return _authService.IsManager();
        }
        public User? GetCurrentUser()
        {
            return _authService.GetCurrentUser();
        }

        public bool Login(string username, string password)
        {
            return _authService.Login(username, password);
        }

        public void Logout()
        {
            _authService.Logout();
        }

        public bool CreateUser(string username, string fullName, string password, string email,  string role = "User")
        {
            return _authService.CreateUser(username, fullName, password, email,  role);
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            return _authService.ChangePassword(userId, oldPassword, newPassword);
        }
        public void UpdateUser(User user)
        {
            var existing = Context.Users.Find(user.Id);
            if (existing == null) return;

            existing.Email = user.Email;
            existing.FullName = user.FullName;
            existing.Username = user.Username;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;
            if (!string.IsNullOrWhiteSpace(user.NewPassword))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.NewPassword);
            }
            _context?.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            _repository.DeleteUser(userId);
        }
        public User? GetUserById(int userId)
        {
            return _repository.GetUserById(userId);
        }
    }
}