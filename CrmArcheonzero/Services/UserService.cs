using System.Collections.Generic;
using System.Linq;
using CrmArcheonzero.Models;
using CrmArcheonzero.Data;

namespace CrmArcheonzero.Services
{
    public class UserService
    {
        private readonly ClientRepository _repository;
        private readonly AuthService _authService;
        public AuthService GetAuthService() => _authService;
        public UserService()
        {
            _repository = new ClientRepository();
            _authService = new AuthService();
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

        public bool CreateUser(string username, string password, string email, string fullName, string role = "User")
        {
            return _authService.CreateUser(username, password, email, fullName, role);
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            return _authService.ChangePassword(userId, oldPassword, newPassword);
        }
        public void UpdateUser(User user)
        {
            _repository.UpdateUser(user);
        }

        public void DeleteUser(int userId)
        {
            _repository.DeleteUser(userId);
        }
    }
}