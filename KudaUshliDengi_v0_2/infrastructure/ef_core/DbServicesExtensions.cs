using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KudaUshliDengi_v0_2.infrastructure.ef_core;

public static class DbServicesExtensions
{
    public static void AddSqlLiteDB(this IServiceCollection services, string filePath)
    {
        services.AddDbContext<SqliteDbContext>(
            options => options.UseSqlite(string.Format("Data Source = {0}", filePath))
        );
    }    
}