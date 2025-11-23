using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result.errors;

public class UserErrors
{
    //301
    public Error NotFoundByTgId(long tgUserId)
        => Error.New($"Не удалось найти пользователя с telegramID = {tgUserId}", 301);

    //302
    public Error UserAlreadyExists(long tgUserId)
        => Error.New($"Пользователь с telegramID = {tgUserId} уже существует", 302);

    //303
    
    //304
    public IError NotAuth(long tgUserId)
        => Error.New($"Пользователь с telegramID = {tgUserId} не авторизован", 304);
}