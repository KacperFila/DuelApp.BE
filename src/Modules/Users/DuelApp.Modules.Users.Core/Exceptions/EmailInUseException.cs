using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Users.Core.Exceptions
{
    internal class EmailInUseException : DuelAppException
    {
        public EmailInUseException() : base("Email is already in use.")
        {
        }
    }
}
