using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.interfaces;

public interface IMessageParser
{
    MessageType MessageType { get; }
    Task<Result> ParseAsync(Message msg, CancellationToken ct = default, Update? context = null);
}