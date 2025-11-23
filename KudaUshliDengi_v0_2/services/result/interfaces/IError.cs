namespace KudaUshliDengi_v0_2.services.result.interfaces;

public interface IError
{
    string Message { get; }
    int Code { get; }
    
    bool IsEmpty { get; }

    string? ToString();
}