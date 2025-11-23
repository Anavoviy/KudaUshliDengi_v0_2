using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using KudaUshliDengi_v0_2.services.try_catcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KudaUshliDengi_v0_2.application.logic.services;

public class UserService(
    IStateStorage<long, UserState> _userStorage,
    SqliteDbContext _context,
    ILogger<UserService> _logger
    ) : IUserService
{
    public async Task<Result<User>> AddAsync(long tgUserId, string tgUsername, long tgChatId, CancellationToken ct = default)
    {
        var resExists = await ExistsAsync(tgUserId, ct);
        if(resExists.IsSuccess)
            return Result<User>.Failure(AllErrors.User.UserAlreadyExists(tgUserId));
        
        var res = await TCatcher.Async(async () =>
        {
            User user = new User(tgUserId, tgChatId, tgUsername);
            await _context.Users.AddAsync(user);
            await _context.CommitAsync();
            return user;
        }, ct);

        return res;
    }

    public async Task<Result<User>> ExistsAsync(long userTgId, CancellationToken ct = default)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(c => c.TgUserId == userTgId, ct);
        return user == null
            ? Result<User>.NotFound(AllErrors.User.NotFoundByTgId(userTgId)) 
            : Result<User>.Success(user); 
    }

    public async Task<Result> CheckStatusAsync(long userId, CancellationToken ct = default)
    {
        var resGet = await _userStorage.GetAsync(userId, ct);
        if (!resGet.IsSuccess)
            return Result.Failure(AllErrors.User.NotAuth(userId));

        return Result.Success();
    }
}