using System;

namespace DuelApp.Shared.Abstractions.Exceptions
{
    public abstract class DuelAppException : Exception
    {
        protected DuelAppException(string message) : base(message)
        {
            
        }
    }
}
