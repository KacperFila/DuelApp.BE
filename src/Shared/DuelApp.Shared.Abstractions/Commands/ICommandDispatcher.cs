using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Commands
{
    public interface ICommandDispatcher
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : class, ICommand;
    }
}