using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.infrastructure.storages;

public static class ServicesExtensions
{
    public static void AddInMemoryStorages(this IServiceCollection services)
    {
        services.AddSingleton<IStateStorage<long, UserState>, UserStateStorage>();
    }
    
}