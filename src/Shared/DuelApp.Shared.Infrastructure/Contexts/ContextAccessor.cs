using System;
using System.Threading;
using DuelApp.Shared.Abstractions.Contexts;

namespace DuelApp.Shared.Infrastructure.Contexts;

public class ContextAccessor : IContextAccessor
{
    private static readonly AsyncLocal<IContext?> _current = new();

    public static void Set(IContext context)
    {
        _current.Value = context;
    }

    public IContext Current =>
        _current.Value ?? throw new InvalidOperationException("Context not initialized");
}