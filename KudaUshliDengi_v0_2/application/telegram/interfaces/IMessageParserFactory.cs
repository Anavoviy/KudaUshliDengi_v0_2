using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.interfaces;

public interface IMessageParserFactory
{
    public Result<IMessageParser> GetParser(MessageType type);
}