using KudaUshliDengi_v0_2.application.logic.services;
using KudaUshliDengi_v0_2.domain.logics.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.application.logic;

public static class ServicesExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFuzzySearchService, FuzzySearchService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOperationService, OperationService>();
    }    
}