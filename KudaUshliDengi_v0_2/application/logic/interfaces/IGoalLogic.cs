using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot.Types;

namespace KudaUshliDengi_v0_2.application.logic.interfaces;

public interface IGoalLogic
{
    //Вывод всех копилок
    Task<Result> ViewAll(Update context, CancellationToken cancellationToken = default);
    
    //Добавление новой копилки
    Task<Result> CreateAsync(Update context, string name, decimal targetAmount, CancellationToken cancellationToken = default);
    
    //Добавление денег в копилку
    Task<Result> IncomeAsync(Update context, string name, decimal amount, CancellationToken cancellationToken = default);
    //Изъятие денег из копилки
    Task<Result> ExpenseAsync(Update context, string name, decimal amount, CancellationToken cancellationToken = default);
    
    //Закрытие копилки
    Task<Result> CancelAsync(Update context, string name, CancellationToken cancellationToken = default);
    //Переименование копилки
    Task<Result> RenameAsync(Update context, string name, string newName, CancellationToken cancellationToken = default);
}