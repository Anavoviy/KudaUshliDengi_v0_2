// See https://aka.ms/new-console-template for more information

using DotNetEnv;
using KudaUshliDengi_v0_2.application.logic;
using KudaUshliDengi_v0_2.application.telegram;
using KudaUshliDengi_v0_2.infrastructure.ef_core;
using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using KudaUshliDengi_v0_2.infrastructure.events;
using KudaUshliDengi_v0_2.infrastructure.mt;
using KudaUshliDengi_v0_2.infrastructure.storages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KudaUshliDengi_v0_2;

internal class Program
{
    static async Task Main(string[] args)
    {
        Env.Load(".env");

        var host = CreateHostBuilder(args).Build();
        
        var scope = host.Services.CreateScope();
        var logger =  scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            
            var context = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();

            if (true) //(await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Apply pending migrations..."); 
                //await context.Database.MigrateAsync();
                logger.LogInformation("👌🏻Database migrate");
            }
            else
            {
                logger.LogInformation("👌🏻Database applied");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"👎🏻Database migrating cancelled with exception: {ex.Message}");
            return;
        }
        scope.Dispose();
        
        await host.RunAsync();
    }
    static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
                {
                    services.AddInMemoryEventProcessing();
                    services.AddSqliteDB(Environment.GetEnvironmentVariable("SQLITE_DB_FILE_PATH"));
                    
                    services.AddInMemoryStorages();
                    
                    services.AddServices();
                    
                    services.AddMTUserLocker();
                    
                    //services.AddScoped<IDomainEventHandler<OperationCreatedEvent>, OperationCreateDomainHandler>();
                    services.AddTelegramUI(Environment.GetEnvironmentVariable("TG_TOKEN"));

                    services.AddLogging(c => c.AddConsole());
                }
            );

        return builder;
    }

}