using KudaUshliDengi_v0_2.domain.events.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.infrastructure.events;

public static class BackgroundEventServicesExtensions
{
    public static void UseInMemoryEventProcessing(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundEventQueue, BackgroundEventQueue>();
        services.AddHostedService<BackgroundEventProcessor>();
        services.AddScoped<IDomainDispatcher, InMemoryDomainDispatcher>();
    }
}