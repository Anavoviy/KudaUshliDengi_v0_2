using System.Collections.Concurrent;
using KudaUshliDengi_v0_2.domain.models;

namespace KudaUshliDengi_v0_2.mt;

public class MTUserLocker : IMTLocker<User>, IAsyncDisposable
{
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _locks = new();
    
    public async Task<IDisposable> WaitAndLockAsync(long id, CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        return new UserLockRelease(semaphore, id);
    }

    public async Task LockAsync(long id, CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
    }

    public Task UnlockAsync(long id, CancellationToken cancellationToken = default)
    {
        if(_locks.TryGetValue(id, out var semaphore))
            semaphore.Release();
        
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (key, semaphore) in _locks)
            semaphore.Dispose();
        _locks.Clear();
    }
}