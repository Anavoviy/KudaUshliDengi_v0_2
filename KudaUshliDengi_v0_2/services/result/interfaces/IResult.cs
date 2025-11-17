namespace KudaUshliDengi_v0_2.services.result.interfaces;

public interface IResult
{
    IError Error { get; }
    bool IsSuccess { get; }
    bool IsFailure { get; }
}