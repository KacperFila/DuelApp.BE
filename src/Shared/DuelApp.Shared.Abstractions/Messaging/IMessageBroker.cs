using System.Threading.Tasks;

namespace DuelApp.Shared.Abstractions.Messaging
{
    public interface IMessageBroker
    {
        Task PublishAsync(params IMessage[] messages);
    }
}