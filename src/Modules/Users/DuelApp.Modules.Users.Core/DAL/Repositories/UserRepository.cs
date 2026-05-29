using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using DuelApp.Modules.Users.Core.Entities;
using DuelApp.Modules.Users.Core.Repositories;

namespace DuelApp.Modules.Users.Core.DAL.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _dbContext;
        private readonly DbSet<User> _users;

        public UserRepository(UsersDbContext dbContext)
        {
            _dbContext = dbContext;
            _users = dbContext.Users;
        }

        public Task<User?> GetByIdAsync(Guid id) => _users.SingleOrDefaultAsync(x => x.Id == id);
        public async Task<User?> GetByKeycloakIdAsync(string id) => await _users.FirstOrDefaultAsync(x => x.KeycloakUserId == id);

        public async Task AddAsync(User user)
        {
            await _users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}