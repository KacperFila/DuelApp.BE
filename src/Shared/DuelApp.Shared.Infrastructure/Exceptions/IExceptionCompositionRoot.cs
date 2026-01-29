using System;
using DuelApp.Shared.Abstractions.Exceptions;

namespace DuelApp.Shared.Infrastructure.Exceptions
{
    internal interface IExceptionCompositionRoot
    {
        ExceptionResponse Map(Exception exception);
    }
}