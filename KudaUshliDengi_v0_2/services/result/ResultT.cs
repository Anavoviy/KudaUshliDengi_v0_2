using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result;

public readonly struct Result<T> : IResult<T>
{
    private readonly ResultStatus _status;
    
    public IError? Error { get; }
    public T? Value { get; }
    
    public bool IsSuccess => _status == ResultStatus.Success;
    public bool IsFailure => _status == ResultStatus.Failure;
    public bool IsNotFound => _status == ResultStatus.NotFound;

    private Result(ResultStatus status, T value = default)
    {
        Value = value;
        Error = BaseError.Empty;
        _status = status;
    }
    
    private Result(ResultStatus status, IError error)
    {
        Value = default;
        Error = error;
        _status = status;
    }
    
    public static Result<T> Success(T value)
        => new Result<T>(ResultStatus.Success, value);

    public static Result<T> Failure(IError error)
        => new Result<T>(ResultStatus.Failure, error);

    public static Result<T> NotFound(IError error)
        => new Result<T>(ResultStatus.NotFound, error);
}