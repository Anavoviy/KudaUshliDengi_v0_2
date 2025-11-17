namespace KudaUshliDengi_v0_2.mt;

public interface IMTLocker<T>
{
    Task<IDisposable> WaitAndLockAsync(long id, CancellationToken cancellationToken = default);
    Task LockAsync(long id, CancellationToken cancellationToken = default);
    Task UnlockAsync(long id, CancellationToken cancellationToken = default);
}