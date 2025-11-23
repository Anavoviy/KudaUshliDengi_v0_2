using KudaUshliDengi_v0_2.application.logic.interfaces;
using KudaUshliDengi_v0_2.application.telegram.builders;
using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using KudaUshliDengi_v0_2.services.try_catcher;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.logics;

public class OperationLogic(
    ITelegramBotClient _bot,
    
    IOperationService operationService,
    IFuzzySearchService fuzzySearch,
    IStateStorage<long, UserState> _stateStorage,
    
    ILogger<OperationLogic> logger
    ) : IOperationLogic
{
    public async Task<Result> CreateIncome(Update context, decimal amount, string categoryName, CancellationToken cancellationToken)
    {
        string mes = "";
        Result res = Result.Success();
        
        if(string.IsNullOrWhiteSpace(categoryName))
            return Result.Failure(AllErrors.Operation.NameCategoryIsRequired(categoryName));
        
        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);

        var searched = await fuzzySearch.SearchCategories(context.Message.From.Id, categoryName, true);
        if (!searched.IsSuccess)
        {
            mes = MessageBuilder.NotFoundCategory(categoryName);
            res = Result.Failure(AllErrors.Category.NotFound(categoryName));
        }else if (searched.Value!.Count > 1)
        {
            mes = MessageBuilder.ExistsManyCategories(searched.Value!);
            res = Result.Failure(AllErrors.Category.NotFound(categoryName));
        }
        else
        {
            var resCreate = await operationService.CreateIncomeAsync(state.Value!.UserId, (Money)amount, searched.Value!.First().Id, cancellationToken);
            if (!resCreate.IsSuccess)
            {
                mes = MessageBuilder.Oops;
                res = Result.Failure(resCreate.Error!);
            }
            else
                mes = MessageBuilder.CreateOperation(resCreate.Value!, categoryName);
        }

        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: cancellationToken);
        return res;
    }

    public async Task<Result> CreateExpense(Update context, decimal amount, string categoryName, CancellationToken cancellationToken)
    {
        string mes = "";
        Result res = Result.Success();
        
        if(string.IsNullOrWhiteSpace(categoryName))
            return Result.Failure(AllErrors.Operation.NameCategoryIsRequired(categoryName));
        
        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);

        var searched = await fuzzySearch.SearchCategories(context.Message.From.Id, categoryName);
        if (!searched.IsSuccess)
        {
            mes = MessageBuilder.NotFoundCategory(categoryName);
            res = Result.Failure(AllErrors.Category.NotFound(categoryName));
        }else if (searched.Value!.Count > 1)
        {
            mes = MessageBuilder.ExistsManyCategories(searched.Value!);
            res = Result.Failure(AllErrors.Category.NotFound(categoryName));
        }
        else
        {
            var resCreate = await operationService.CreateIncomeAsync(state.Value!.UserId, (Money)amount, searched.Value!.First().Id, cancellationToken);
            if (!resCreate.IsSuccess)
            {
                mes = MessageBuilder.Oops;
                res = Result.Failure(resCreate.Error!);
            }
            else
                mes = MessageBuilder.CreateOperation(resCreate.Value!, searched.Value!.First().Name);
        }

        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: cancellationToken);
        return res;
    }
}