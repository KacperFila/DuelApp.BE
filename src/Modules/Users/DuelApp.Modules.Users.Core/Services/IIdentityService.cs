using System.Threading.Tasks;
using System;
using DuelApp.Modules.Users.Core.DTO;
using DuelApp.Shared.Abstractions.Auth;

namespace DuelApp.Modules.Users.Core.Services
{
    public interface IIdentityService
    {
        Task<AccountDto> GetAsync(Guid id);
        Task<JsonWebToken> SignInAsync(SignInDto dto);
        Task SignUpAsync(SignUpDto dto);
    }
}