using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.telegram.services;

public class Auth(
    ITelegramBotClient _bot,
    IStateStorage<long, UserState> _userStorage,
    IUserService _userService,
    ICategoryService _categoryService
    ) : IAuthService
{
    public async Task<Result<UserState>> Login(User tgUser, ChatId chatId)
    {
        var exists = await _userService.ExistsAsync(tgUser.Id);
        if (!exists.IsSuccess)
            return await Register(tgUser, chatId);
        
        var resAuth = await _userService.CheckStatusAsync(tgUser.Id);
        if (resAuth.IsFailure)
        {
            var res = await _userStorage.SetAsync(tgUser.Id, new UserState(exists.Value!.Id, UserStatus.Main));
            if (!res.IsSuccess)
                return Result<UserState>.Failure(res.Error!);
        }

        var getState = await _userStorage.GetAsync(tgUser.Id);
        if(getState.IsFailure)
            return Result<UserState>.Failure(getState.Error!);
        
        return Result<UserState>.Success(getState.Value!);
    }

    public async Task<Result<UserState>> Register(User tgUser, ChatId chatId)
    {
        var exists = await _userService.ExistsAsync(tgUser.Id);
        if(exists.IsSuccess)
            return await Login(tgUser, chatId);

        var res = await _userService.AddAsync(tgUser.Id, tgUser.Username!, (long)chatId.Identifier!);
        if (!res.IsSuccess)
            return Result<UserState>.Failure(res.Error!);
        var resInit = await _categoryService.CreateBasicCategoriesForUser(res.Value!.Id);
        if (!resInit.IsSuccess)
            return Result<UserState>.Failure(resInit.Error!);
        
        return await Login(tgUser, chatId);
    }
}