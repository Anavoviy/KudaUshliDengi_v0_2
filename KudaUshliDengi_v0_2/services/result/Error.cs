using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result;

public readonly struct Error : IError
{
    public string Message { get; }
    public int Code { get; }

    public bool IsEmpty => Code == 0;

    private Error(string message, int code)
    {
        Message = message;
        Code = code;
    }

    public static Error New(string message, int code)
        => new Error(message, code);

    public static Error Empty
        => new Error("default", 0);

    public string? ToString() => $"\n\tCode: {Code}\n\tMessage: {Message}";
}