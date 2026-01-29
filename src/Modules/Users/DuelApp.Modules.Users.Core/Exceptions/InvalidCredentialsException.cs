using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Modules.Users.Core.Exceptions
{
    internal class InvalidCredentialsException : DuelAppException
    {
        public InvalidCredentialsException() : base("Invalid credentials.")
        {
        }
    }
}
