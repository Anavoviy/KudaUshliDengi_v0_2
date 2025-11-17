using System.Threading.Channels;
using KudaUshliDengi_v0_2.mt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;

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
                
                Console.WriteLine(lastUpdateId);
            }
            
            await Task.Delay(700, ct);
        }
    }
    private async Task Processor(int id, CancellationToken ct)
    {
        await foreach (var update in queue.Reader.ReadAllAsync(ct))
        {
            using var scope = provider.CreateScope();
            
            var mtLocker = scope.ServiceProvider.GetService<IMTLocker<User>>();
            if (mtLocker is null)
                throw new NotImplementedException("В DI отсутствует IMTLocker<User>!");
            
            using (await mtLocker.WaitAndLockAsync(update.Message.From.Id, ct))
            {
                //TODO: Добавить обработку   
            }
            //TODO: Добавить логи
            Console.WriteLine($"Задача №{id} обработала сообщение {update.Message?.Text ?? "где нет текста"} от пользователя {update.Message.From.Username}#{update.Message.From.Id}");
        }
    }
}