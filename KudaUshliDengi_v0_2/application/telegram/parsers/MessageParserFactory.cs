using KudaUshliDengi_v0_2.application.telegram.interfaces;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.parsers;

public class MessageParserFactory : IMessageParserFactory
{
    private readonly Dictionary<MessageType, IMessageParser> _parsers;

    public MessageParserFactory(IEnumerable<IMessageParser> parsers)
    {
        _parsers = parsers.ToDictionary(x => x.MessageType, x => x);
    }        
    
    public Result<IMessageParser> GetParser(MessageType type)
    {
        if(!_parsers.ContainsKey(type))
            return Result<IMessageParser>.NotFound(Error.New($"Нет обработчика для типа сообщения: {type}", 121));
        
        return Result<IMessageParser>.Success(_parsers[type]);
    }
}