using DuelApp.Shared.Abstractions.Contexts;

namespace DuelApp.Shared.Infrastructure.Contexts
{
    internal interface IContextFactory
    {
        IContext Create();
    }
}