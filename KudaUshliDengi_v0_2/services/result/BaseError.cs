using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result;

public readonly struct BaseError : IError
{
    public string Message { get; }
    public int Code { get; }

    public bool IsEmpty => Code == 0;

    private BaseError(string message, int code)
    {
        Message = message;
        Code = code;
    }

    public static BaseError New(string message, int code)
        => new BaseError(message, code);

    public static BaseError Empty
        => new BaseError("default", 0);
}