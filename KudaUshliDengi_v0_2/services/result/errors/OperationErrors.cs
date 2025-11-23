using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result.errors;

public class OperationErrors
{
    //200
    
    //201
    public IError NotFound(string? name = null)
        => Error.New(name is null ? $"У вас отсутствуют цели!" : $"Не найдена цель с именем {name}", 201);
    
    //202
    //203
    //204
    //205

    //206
    public IError Exists(string name)
        => Error.New($"Уже существует незакрытая цель: {name}", 206);

    //207
    public IError NameCategoryIsRequired(string name)
        => Error.New($"Имя категории обязательно! Было передано: {name}", 207);
    
    //208
    public IError SumItemsMoreMainAmount(Money amount, decimal sumItems)
        => Error.New($"Сумма подопераций ({sumItems}) больше общей суммы операции ({amount})!", 208);
}