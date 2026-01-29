using System.Threading.Channels;
using DuelApp.Shared.Abstractions.Messaging;

namespace DuelApp.Shared.Infrastructure.Messaging.Dispatchers
{
    public interface IMessageChannel
    {
        ChannelReader<IMessage> Reader { get; }
        ChannelWriter<IMessage> Writer { get; }
    }
}