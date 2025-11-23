using KudaUshliDengi_v0_2.application.logic.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.application.telegram.logics;

public static class TgLogicsServicesExtensions
{
    public static void AddTgLogic(this IServiceCollection services)
    {
        services.AddScoped<ICategoryLogic, CategoryLogic>();
        services.AddScoped<IGoalLogic, GoalLogic>();
    }
}