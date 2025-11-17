using System.Collections.Concurrent;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.infrastructure.storages;

public class UserStateStorage : IStateStorage<long, UserState>
{
    private readonly ConcurrentDictionary<long, UserState> _store = new();
    
    public Task<Result<UserState>> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(userId, out var result))
            return Task.FromResult(Result<UserState>.Success(result));
        return Task.FromResult(Result<UserState>.NotFound(BaseError.New("User не найден", 121)));
    }
    public Task<Result> RemoveAsync(long userId, CancellationToken ct = default)
    {
        if (_store.ContainsKey(userId))
            _store.TryRemove(userId, out _);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> SetAsync(long userId, UserState state, CancellationToken ct = default)
    {
        _store[userId] = state;

        return Task.FromResult(Result.Success());
    }

}