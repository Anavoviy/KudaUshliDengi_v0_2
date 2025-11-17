namespace KudaUshliDengi_v0_2.services.result.interfaces;

public interface IResult<T>
{
    IError Error { get; }
    T Value { get; }
    
    bool IsSuccess { get; }
    bool IsFailure { get; }
    bool IsNotFound { get; } 
}