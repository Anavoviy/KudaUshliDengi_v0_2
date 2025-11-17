using KudaUshliDengi_v0_2.application.telegram.interfaces;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.telegram.parsers;

public class MainMessageParser(
    IMessageParserFactory factory
) : IMainMessageParser
{
    public async Task<Result> ParseAsync(Message msg, CancellationToken ct = default, Update? context = null)
    {
        var parser = factory.GetParser(msg.Type);
        if (parser.IsNotFound)
            return Result.Failure(parser.Error);
        
        return await parser.Value!.ParseAsync(msg, ct, context);
    }
}