using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.mt;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.infrastructure.mt;

public static class MtServicesExtensions
{
    public static void AddMTUserLocker(this IServiceCollection services)
    {
        services.AddSingleton<IMTLocker<User>, MTUserLocker>();
    }
}