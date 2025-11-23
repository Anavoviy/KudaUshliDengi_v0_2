using KudaUshliDengi_v0_2.application.logic.interfaces;
using KudaUshliDengi_v0_2.application.logic.services;
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

public class GoalLogic(
    ITelegramBotClient _bot,
    
    IFuzzySearchService _fuzzySearch,
    IGoalService _goalService,
    IStateStorage<long, UserState> _stateStorage,
    
    ILogger<GoalLogic> _logger) : IGoalLogic
{
    public async Task<Result> ViewAll(Update context, CancellationToken cancellationToken = default)
    {
        string mes = "";
        Result res = Result.Success();
        
        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);
        
        var goals = await _goalService.GetAllAsync(state.Value!.UserId, cancellationToken);
        if (goals.IsFailure)
        {
            mes = MessageBuilder.Oops;
            res = Result.Failure(goals.Error!);
        }
        else if (goals.IsNotFound)
            mes = MessageBuilder.NotFoundGoals;
        else if(goals.IsSuccess)
            mes = MessageBuilder.ListGoals(goals.Value);
        
        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown);
        return res;
    }
    
    public async Task<Result> CreateAsync(Update context, string name, decimal targetAmount, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(name))
            return Result.Failure(AllErrors.Goal.NameIsRequired(name));

        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);
        
        var resSearchGoals = await _fuzzySearch.SearchGoals(context.Message.From!.Id, name);
        if (resSearchGoals.IsSuccess && resSearchGoals.Value!.Any(g => g.Name == name))
            return Result.Failure(AllErrors.Goal.Exists(name));

        var resCreate = await _goalService.CreateAsync(state.Value!.UserId, name, (Money)targetAmount, cancellationToken);

        if (resCreate.IsFailure)
        {
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.Oops, ParseMode.Markdown);
            return Result.Failure(resCreate.Error!);
        }

        await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.CreateGoal(resCreate.Value!), ParseMode.Markdown);
        
        return Result.Success();
    }

    public Task<Result> IncomeAsync(Update context, string name, decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ExpenseAsync(Update context, string name, decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> CancelAsync(Update context, string name, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(name))
            return Result.Failure(AllErrors.Goal.NameIsRequired(name));

        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);
        
        var resCancel = await _goalService.CancelAsync(state.Value!.UserId, name, cancellationToken);

        if(resCancel.IsFailure)
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.Oops, ParseMode.Markdown);
        else 
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.CancelGoal(name), ParseMode.Markdown);
        
        return resCancel;
    }

    public async Task<Result> RenameAsync(Update context, string name, string newName, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(name))
            return Result.Failure(AllErrors.Goal.NameIsRequired(name));
        if(string.IsNullOrWhiteSpace(newName))
            return Result.Failure(AllErrors.Goal.NameIsRequired(newName));

        var state = await _stateStorage.GetAsync(context.Message.From.Id);
        if(!state.IsSuccess)
            return Result.Failure(state.Error!);
        
        var resRename = await _goalService.RenameAsync(state.Value!.UserId, name, newName, cancellationToken);

        if (resRename.IsFailure)
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.Oops, ParseMode.Markdown);
        else
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.GoalRename(name, newName),
                ParseMode.Markdown);
        
        return resRename;
    }
}