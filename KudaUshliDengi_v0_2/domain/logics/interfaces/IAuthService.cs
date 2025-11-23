using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface IAuthService
{
    public Task<Result<UserState>> Login(User tgUser, ChatId chatId);
    public Task<Result<UserState>> Register(User tgUser, ChatId chatId); 
}