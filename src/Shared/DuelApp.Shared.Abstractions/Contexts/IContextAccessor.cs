namespace DuelApp.Shared.Abstractions.Contexts;

public interface IContextAccessor
{
    IContext Current { get; }
}