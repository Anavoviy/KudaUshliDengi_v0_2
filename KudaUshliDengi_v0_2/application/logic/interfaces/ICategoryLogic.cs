using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.logic.interfaces;

public interface ICategoryLogic
{
    Task<Result> GetAllAsync(Update context, bool grouping, CancellationToken ct);

    Task<Result> CreateIncomeAsync(Update context, string categoryName, CancellationToken ct);

    Task<Result> CreateExpenseAsync(Update context, string categoryName, CancellationToken ct);
    Task<Result> CreateExpenseAsync(Update context, string categoryName, string parentCategoryName,
        CancellationToken ct);
    
    Task<Result> RenameAsync(Update context, string oldCategoryName, string newCategoryName, CancellationToken ct);
    
    Task<Result> DeleteAsync(Update context, string categoryName, CancellationToken ct);
}