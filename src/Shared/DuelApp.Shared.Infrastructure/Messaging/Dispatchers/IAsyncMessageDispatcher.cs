using System.Threading.Tasks;
using DuelApp.Shared.Abstractions.Messaging;

namespace DuelApp.Shared.Infrastructure.Messaging.Dispatchers
{
    internal interface IAsyncMessageDispatcher
    {
        Task PublishAsync<TMessage>(TMessage message) where TMessage : class, IMessage;
    }
}