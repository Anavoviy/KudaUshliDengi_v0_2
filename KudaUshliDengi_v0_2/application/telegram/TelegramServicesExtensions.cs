using KudaUshliDengi_v0_2.application.telegram.interfaces;
using KudaUshliDengi_v0_2.application.telegram.parsers;
using KudaUshliDengi_v0_2.telegram.background;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace KudaUshliDengi_v0_2.application.telegram;

public static class TelegramServicesExtensions
{
    public static void AddTelegramUI(this IServiceCollection services, string token)
    {
        services.AddHttpClient<ITelegramBotClient>().RemoveAllLoggers()
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddTypedClient<ITelegramBotClient>((httpClient, provider) 
                => new  TelegramBotClient(token, httpClient)
            );

        services.AddHostedService<TelegramBackgroundService>();

        //services.AddScoped<Auth>(); //TODO: Добавить Auth
        
        
        // Добавление MessageParsers
        services.Scan(scan => scan
            .FromAssemblyOf<IMessageParser>()
            .AddClasses(cls => cls.AssignableTo<IMessageParser>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );
        services.AddScoped<IMessageParserFactory, MessageParserFactory>();
        services.AddScoped<IMainMessageParser, MainMessageParser>();
    }
}