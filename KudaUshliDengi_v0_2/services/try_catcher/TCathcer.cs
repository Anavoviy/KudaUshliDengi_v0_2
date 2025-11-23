using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.try_catcher.interfaces;

namespace KudaUshliDengi_v0_2.services.try_catcher;

public abstract class TCatcher : ITCatcher
{
    public static async Task<Result> Async(Func<Task> action, CancellationToken ct = default)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.New(ex.Message, ex.HResult));
        }
    }

    public Result Sync(Action action)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.New(ex.Message, ex.HResult));
        }
    }

    public static async Task<Result<TResult>> Async<TResult>(Func<Task<TResult?>> func, CancellationToken ct = default)
    {
        try
        {
            var res = await func();
            if (res is null)
                return Result<TResult>.NotFound(Error.New("Не удалось найти. Вернулось значение Null", 101)); //TODO: Придумай ты уже коды ошибок
            return Result<TResult>.Success(res!);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(Error.New(ex.Message, ex.HResult));
        }
    }

    public Result<TResult> Sync<TResult>(Func<TResult?> func)
    {
        try
        {
            var res = func();
            if (res is null)
                return Result<TResult>.NotFound(Error.New("Не удалось найти. Вернулось значение Null", 101));  //TODO: Придумай ты уже коды ошибок
            return Result<TResult>.Success(res!);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(Error.New(ex.Message, ex.HResult));
        }
    }
}