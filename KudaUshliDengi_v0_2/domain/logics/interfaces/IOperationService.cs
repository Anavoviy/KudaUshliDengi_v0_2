using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface IOperationService
{
    //Добавление +5000 подарок
    Task<Result<Operation>> CreateIncomeAsync(UserId userId, Money amount, CategoryId? categoryId = null, CancellationToken cancellationToken = default);
    
    //Добавление 5000 кофе
    Task<Result<Operation>> CreateExpenseAsync(UserId userId, Money amount, CategoryId? categoryId = null, CancellationToken cancellationToken = default);
    //Добавление 5000 продукты: 1000 алкоголь 500 вкусняшки
    Task<Result<Operation>> CreateExpenseAsync(UserId userId, Money amount, CategoryId categoryId, List<OperationData> items, CancellationToken cancellationToken = default);
    
    //TODO: Нахуя это нужно?
    Task<Result<Operation>> GetLastAsync(UserId userId, CancellationToken cancellationToken = default);
    //Показать последние 10 операций
    Task<Result<List<Operation>>> GetLast10Async(UserId userId, CancellationToken cancellationToken = default);
    
    //TODO: Нахуя это здесь??? Это же должно быть в отчётах!!!
    Task<Result<List<Operation>>> GetAllsOnDateRangeAsync(UserId userId, DateOnly start, DateOnly end, CancellationToken cancellationToken = default);
    
    //Удаление последней операции
    Task<Result> CancelLastAsync(UserId userId, CancellationToken cancellationToken = default);
    //Изменение определённой операции
    Task<Result> EditAsync(UserId userId, OperationId operationId, Money amount, CategoryId? categoryId = null, List<OperationData>? items = null, CancellationToken cancellationToken = default);
}