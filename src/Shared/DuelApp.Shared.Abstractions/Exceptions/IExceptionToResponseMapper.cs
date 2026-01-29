using System;

namespace DuelApp.Shared.Abstractions.Exceptions
{
    public interface IExceptionToResponseMapper
    {
        ExceptionResponse Map(Exception exception);
    }
}