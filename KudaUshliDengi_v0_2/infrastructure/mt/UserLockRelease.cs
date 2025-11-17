namespace KudaUshliDengi_v0_2.mt;

public class UserLockRelease : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly long _userId;

    public UserLockRelease(SemaphoreSlim semaphore, long userId)
    {
        _semaphore = semaphore;
        _userId = userId;
    }
    
    public void Dispose()
    {
        _semaphore.Release();
    }
}