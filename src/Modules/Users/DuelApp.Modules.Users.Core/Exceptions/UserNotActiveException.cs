using System;
using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Users.Core.Exceptions
{
    public class UserNotActiveException : DuelAppException
    {
        public Guid UserId { get; }
        public UserNotActiveException(Guid userId) : base($"User with ID: '{userId}' is not active.")
        {
            UserId = userId;
        }
    }
}