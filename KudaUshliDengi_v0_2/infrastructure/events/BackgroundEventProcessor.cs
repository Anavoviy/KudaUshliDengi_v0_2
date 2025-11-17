using KudaUshliDengi_v0_2.domain.events.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KudaUshliDengi_v0_2.infrastructure.events;

public class BackgroundEventProcessor(
    IBackgroundEventQueue eventQueue,
    IServiceScopeFactory scopeFactory
    //,ILogger<BackgroundEventProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var @event = await eventQueue.DequeueAsync(stoppingToken);
                await ProcessEventAsync(@event, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error processing domain event!");
            }
        }
    }

    private async Task ProcessEventAsync(IDomainEvent @event, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
        var handlers =  scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            await (Task)method.Invoke(handler, new object[] { @event, ct});
        }
    }
}