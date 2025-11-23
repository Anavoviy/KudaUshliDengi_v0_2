using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using KudaUshliDengi_v0_2.services.try_catcher;
using Microsoft.Extensions.Logging;

namespace KudaUshliDengi_v0_2.application.logic.services;

public class OperationService(
    SqliteDbContext context,
    
    IFuzzySearchService fuzzySearch,
    
    ILogger<OperationService> logger
    ) : IOperationService
{
    public async Task<Result<Operation>> CreateIncomeAsync(UserId userId, Money amount, CategoryId? categoryId = null,
        CancellationToken cancellationToken = default)
        => await TCatcher.Async(async () =>
        {
            Operation operation = categoryId is null
                ? new Operation(userId, amount, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow))
                : new Operation(userId, amount, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow),
                    categoryId);
            
            await context.Operations.AddAsync(operation, cancellationToken);
            await context.CommitAsync(cancellationToken);
            
            return operation; 
        }, cancellationToken);

    public async Task<Result<Operation>> CreateExpenseAsync(UserId userId, Money amount, CategoryId? categoryId = null,
        CancellationToken cancellationToken = default)
        => await TCatcher.Async(async () =>
        {
            Operation operation = categoryId is null
                ? new Operation(userId, amount, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow))
                : new Operation(userId, amount, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow),
                    categoryId);
            
            await context.Operations.AddAsync(operation, cancellationToken);
            await context.CommitAsync(cancellationToken);
            
            return operation; 
        }, cancellationToken);

    public async Task<Result<Operation>> CreateExpenseAsync(UserId userId, Money amount, CategoryId categoryId, List<OperationData> items,
        CancellationToken cancellationToken = default)
    {
        if(items.Count == 0)
            await CreateExpenseAsync(userId, amount, categoryId, cancellationToken);
        
        
        Operation operation = new Operation(userId, amount, TransactionType.Expense,
            DateOnly.FromDateTime(DateTime.UtcNow),
            categoryId);

        decimal sumItems = items.Select(o => o.Amount).Sum(o => o.Amount);
        if (amount < sumItems)
            return Result<Operation>.Failure(AllErrors.Operation.SumItemsMoreMainAmount(amount, sumItems));
            
        return Result<Operation>.Success(operation);
    }

    public Task<Result<Operation>> GetLastAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Operation>>> GetLast10Async(UserId userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<Operation>>> GetAllsOnDateRangeAsync(UserId userId, DateOnly start, DateOnly end,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> CancelLastAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> EditAsync(UserId userId, OperationId operationId, Money amount, CategoryId? categoryId = null, List<OperationData>? items = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}