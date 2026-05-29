using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DuelApp.Modules.Users.Core.Services;

public interface IAccountService
{
    public Task<string?> UploadAvatar(Guid userId, IFormFile file);
    
    public string GetUserAvatar(Guid userId);
}