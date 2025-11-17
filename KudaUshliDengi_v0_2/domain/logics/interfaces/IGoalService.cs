using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface IGoalService
{
    //Добавление новой копилки    
    Task CreateAsync(UserId userId, string name, Money targetAmount, CancellationToken cancellationToken = default);
    
    //Добавление денег в копилку
    Task<Result> IncomeAsync(UserId userId, string name, Money amount, CancellationToken cancellationToken = default);
    //Изъятие денег из копилки
    Task<Result> ExpenseAsync(UserId userId, string name, Money amount, CancellationToken cancellationToken = default);
    
    //Закрытие копилки
    Task CancelAsync(UserId userId, string name, CancellationToken cancellationToken = default);
    //Переименование копилки
    Task RenameAsync(UserId userId, string name, string newName, CancellationToken cancellationToken = default);
}