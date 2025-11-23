using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.logic.interfaces;

public interface IOperationLogic
{
    public Task<Result> CreateIncome(Update context, decimal amount, string categoryName, CancellationToken cancellationToken);
    public Task<Result> CreateExpense(Update context, decimal amount, string categoryName, CancellationToken cancellationToken); 
}