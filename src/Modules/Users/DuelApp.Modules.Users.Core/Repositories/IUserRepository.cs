using System.Threading.Tasks;
using System;
using DuelApp.Modules.Users.Core.Entities;

namespace DuelApp.Modules.Users.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByProfileIdAsync(Guid profileId);
    Task<User?> GetByUserIdAsync(Guid userId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
