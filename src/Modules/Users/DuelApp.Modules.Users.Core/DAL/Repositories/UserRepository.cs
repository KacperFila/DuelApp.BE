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

        public Task<User?> GetByProfileIdAsync(Guid profileId) => _users.SingleOrDefaultAsync(x => x.ProfileId == profileId);
        public async Task<User?> GetByUserIdAsync(Guid userId) => await _users.FirstOrDefaultAsync(x => x.UserId == userId);

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
