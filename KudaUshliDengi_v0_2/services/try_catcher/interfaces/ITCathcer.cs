using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.services.try_catcher.interfaces;

public interface ITCatcher
{
    static Task<Result> Async(Func<Task> action, CancellationToken ct = default)
    {
        throw new NotImplementedException(); 
    }
    Result Sync(Action action);
    
    
    static Task<Result<TResult>> Async<TResult>(Func<Task<TResult?>> func, CancellationToken ct = default)
    {
        throw new NotImplementedException(); 
    }
    Result<TResult> Sync<TResult>(Func<TResult?> func);
}