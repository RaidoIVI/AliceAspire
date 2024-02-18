using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Extensions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder, Action<JsonSerializerOptions> configure)
    {
        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            configure(o.JsonSerializerOptions);
        });

        return eventBusBuilder;
    }

    public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(this IEventBusBuilder eventBusBuilder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        // Использовать службы с ключами для регистрации нескольких обработчиков для одного и того же типа событий.
        // IKeyedServiceProvider.GetKeyedService<IIntegrationEventHandler>(typeof(T)) для получеия всех обработчиков.
        
        eventBusBuilder.Services.AddKeyedTransient<IIntegrationEventHandler, TH>(typeof(T));

        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            // Отслеживание все зарегистрированные типы событий и сопоставление их имен.
            // and we don't want to do Type.GetType, so we keep track of the name mapping here.

            // Этот список используется для подписки на события базовой реализации брокера сообщений.
            o.EventTypes[typeof(T).Name] = typeof(T);
        });

        return eventBusBuilder;
    }
}
