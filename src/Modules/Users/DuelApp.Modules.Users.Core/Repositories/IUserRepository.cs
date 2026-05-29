using System.Threading.Tasks;
using System;
using DuelApp.Modules.Users.Core.Entities;

namespace DuelApp.Modules.Users.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByKeycloakIdAsync(string id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}