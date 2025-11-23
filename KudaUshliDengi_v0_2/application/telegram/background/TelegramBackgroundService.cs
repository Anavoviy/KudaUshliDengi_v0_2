using System.Threading.Channels;
using KudaUshliDengi_v0_2.application.telegram.interfaces;
using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.infrastructure.mt;
using KudaUshliDengi_v0_2.mt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = KudaUshliDengi_v0_2.domain.models.User;

namespace KudaUshliDengi_v0_2.telegram.background;

public class TelegramBackgroundService(IServiceProvider provider) : BackgroundService
{
    private readonly Channel<Update> queue = Channel.CreateUnbounded<Update>();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiver = Receiver(stoppingToken); // Создание писателя update-ов

        int count = 10;
        Task[] processors = new Task[count];
        for (int i = 0; i < count; i++)
            processors[i] = Processor(i+1, stoppingToken);  // Создание исполнителей update-ов
        
        await Task.WhenAll(receiver, Task.WhenAll(processors));
    }
    
    private async Task Receiver(CancellationToken ct)
    {
        int lastUpdateId = 0;
        var bot = provider.GetRequiredService<ITelegramBotClient>();
        
        while (!ct.IsCancellationRequested)
        {
            var updates = await bot.GetUpdates(lastUpdateId + 1, cancellationToken: ct);
            foreach (var update in updates)
            {
                if (ct.IsCancellationRequested)
                    break;
                lastUpdateId = update.Id;
                await queue.Writer.WriteAsync(update, ct);
            }
            
            await Task.Delay(700, ct);
        }
    }
    private async Task Processor(int id, CancellationToken ct)
    {
        await foreach (var update in queue.Reader.ReadAllAsync(ct))
        {
            using var scope = provider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TelegramBackgroundService>>();
            try
            {
                var mtLocker = scope.ServiceProvider.GetService<IMTLocker<User>>();
                if (mtLocker is null)
                {
                    logger.LogCritical("В DI отсутствует IMTLocker<User>!");
                    //throw new NotImplementedException("В DI отсутствует IMTLocker<User>!");
                }

                using (await mtLocker.WaitAndLockAsync(update.Message!.From!.Id, ct))
                {
                    IAuthService? auth = scope.ServiceProvider.GetService<IAuthService>();
                    if (auth is null)
                    {
                        logger.LogCritical("В DI отсутствует IAuthService!");
                        //throw new NotImplementedException("В DI отсутствует IAuthService!");
                    }

                    //TODO: Добавить обработку
                    var resAuth = await auth!.Login(update.Message.From!, update.Message.Chat.Id);
                    if (!resAuth.IsSuccess)
                    {
                        logger.LogError(resAuth.Error!.ToString());
                        continue;
                    }

                    var parser = scope.ServiceProvider.GetService<IMainMessageParser>();
                    if (parser is null)
                    {
                        logger.LogCritical("В DI отсутствует IMainMessageParser!");
                        //throw new NotImplementedException("В DI отсутствует IMainMessageParser!");
                    }

                    var resProcessing = await parser.ParseAsync(update.Message, ct, update);
                    if (!resProcessing.IsSuccess)
                        logger.LogError(
                            $"Задача №{id} не обработала сообщение {update.Message?.Text ?? "где нет текста"} от пользователя {update.Message.From.Username}#{update.Message.From.Id}!\nОшибка: " +
                            resProcessing.Error!.ToString());
                    else
                        logger.LogInformation(
                            $"Задача №{id} обработала сообщение {(update.Message.Text is null ? $"\"{update.Message?.Text}\"" : "где нет текста")} от пользователя {update.Message.From.Username}#{update.Message.From.Id}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            
            //TODO: Добавить логи
            
        }
    }
}