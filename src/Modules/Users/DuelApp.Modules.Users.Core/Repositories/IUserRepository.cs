using System.Threading.Tasks;
using System;
using DuelApp.Modules.Users.Core.Entities;

namespace DuelApp.Modules.Users.Core.Repositories
{
    internal interface IUserRepository
    {
        Task<User> GetAsync(Guid id);
        Task<User> GetAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}