
using Microsoft.Extensions.DependencyInjection;
using DuelApp.Shared.Abstractions.Messaging;
using DuelApp.Shared.Infrastructure.Messaging.Brokers;
using DuelApp.Shared.Infrastructure.Messaging.Dispatchers;

namespace DuelApp.Shared.Infrastructure.Messaging
{
    internal static class Extensions
    {
        private const string SectionName = "messaging";
        
        internal static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBroker, MessageBroker>();
            services.AddSingleton<IMessageChannel, MessageChannel>();
            services.AddSingleton<IAsyncMessageDispatcher, AsyncMessageDispatcher>();

            var messagingOptions = services.GetOptions<MessagingOptions>(SectionName);
            services.AddSingleton(messagingOptions);

            if (messagingOptions.UseBackgroundDispatcher)
            {
                services.AddHostedService<BackgroundDispatcher>();
            }
            
            return services;
        }
    }
}