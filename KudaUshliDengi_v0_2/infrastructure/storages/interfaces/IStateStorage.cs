using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.infrastructure.storages.interfaces;

public interface IStateStorage<TKey, TValue>
{
    public Task<Result<TValue>> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<Result> SetAsync(TKey id, TValue state, CancellationToken ct = default);
    Task<Result> RemoveAsync(TKey id, CancellationToken ct = default);
}
