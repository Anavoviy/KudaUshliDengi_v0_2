using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result;

public readonly struct Result : IResult
{
    private readonly ResultStatus _status;
    public IError? Error { get; }

    public bool IsSuccess => _status == ResultStatus.Success;
    public bool IsFailure => _status == ResultStatus.Failure;

    private Result(ResultStatus status, IError? error = null)
    {
        _status = status;
        Error = error;
    }

    public static Result Success()
        => new Result(ResultStatus.Success, BaseError.Empty);

    public static Result Failure(IError error)
        => new Result(ResultStatus.Failure, error);
}