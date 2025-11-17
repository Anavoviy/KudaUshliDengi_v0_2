using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.telegram.interfaces;

public interface IMainMessageParser
{
    Task<Result> ParseAsync(Message msg, CancellationToken ct = default, Update? context = null);
}