using System.Text.Json;
using KudaUshliDengi_v0_2.application.logic.interfaces;
using KudaUshliDengi_v0_2.application.telegram.builders;
using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.infrastructure.storages.context_items;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.logics;

public class CategoryLogic(
    ITelegramBotClient _bot, 
    IStateStorage<long, UserState> _userStateStorage,
    ICategoryService _categoryService, 
    IUserService _userService,
    
    IFuzzySearchService _fuzzySearch,
    
    ILogger<CategoryLogic> _logger) : ICategoryLogic
{
    public async Task<Result> GetAllAsync(Update context, bool grouping, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());

        string mes = "";
        
        var categories = await _categoryService.GetAsync(state.Value!.UserId, grouping, ct);
        if (categories.IsNotFound)
            mes = "У вас отсутствуют категории🥹";
        else
            mes = MessageBuilder.ListCategories(categories.Value!);

        await _bot.SendMessage(context.Message.Chat.Id, mes, parseMode: ParseMode.Markdown, cancellationToken: ct);
        return Result.Success();
    }

    public async Task<Result> CreateIncomeAsync(Update context, string categoryName, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());

        var exists = await _categoryService.ExistsIncomeCategory(state.Value!.UserId, categoryName, ct);
        if (exists)
        {
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.CategoryExists(categoryName));
            return Result.Failure(AllErrors.Category.NotCreated(categoryName));
        }
        
        var existsParent = await _categoryService.ExistsCategory(state.Value!.UserId, "доходы", ct);
        if (!existsParent)
        {
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.NotFoundCategory("доходы", TransactionType.Income));
            return Result.Failure(AllErrors.Category.NotCreated(categoryName));
        }
        
        var resCreate = await _categoryService.CreateAsync(state.Value!.UserId, categoryName, "доходы");
        if (resCreate.IsFailure)
        {
            _logger.LogError(resCreate.Error!.ToString());
            return Result.Failure(resCreate.Error!);
        }

        _logger.LogInformation($"Пользователь {state.Value.UserId.value} успешно создал категорию \"{categoryName}\"");

        var msg = MessageBuilder.CategoryCreate(categoryName, "доходы");
        await _bot.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
        return Result.Success();
    }
    
    public async Task<Result> CreateExpenseAsync(Update context, string categoryName, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());

        var exists = await _categoryService.ExistsCategory(state.Value!.UserId, categoryName, ct);
        if (exists)
        {
            await _bot.SendMessage(context.Message.Chat.Id, MessageBuilder.CategoryExists(categoryName));
            return Result.Failure(AllErrors.Category.NotCreated(categoryName));
        }
             
        var resCreate = await _categoryService.CreateAsync(state.Value!.UserId, categoryName);
        if (resCreate.IsFailure)
        {
            _logger.LogError(resCreate.Error!.ToString());
            return Result.Failure(resCreate.Error!);
        }

        _logger.LogInformation($"Пользователь {state.Value.UserId.value} успешно создал категорию \"{categoryName}\"");

        var msg = MessageBuilder.CategoryCreate(categoryName);
        await _bot.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
        return Result.Success();
    } 
    
    public async Task<Result> CreateExpenseAsync(Update context, string categoryName, string parentCategoryName, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());

        bool isSuccess = true;
        string mes = MessageBuilder.Oops;
        Result res = Result.Success();
        
        var resExistsParent =  await _fuzzySearch.SearchCategories(context.Message.Chat.Id, parentCategoryName);
        if (resExistsParent.IsNotFound)
        {
            mes = MessageBuilder.NotFoundCategory(parentCategoryName, TransactionType.Expense);
            res = Result.Failure(AllErrors.Category.NotCreated(categoryName));
            isSuccess = false;
        } else if (resExistsParent.Value.Count() > 1)
        {
            mes = MessageBuilder.ExistsManyParentCategories(parentCategoryName, resExistsParent.Value);
            res = Result.Failure(AllErrors.Category.NotCreated(categoryName));
            isSuccess = false;
        }

        if (await _categoryService.ExistsCategory(state.Value!.UserId, categoryName, ct))
        {
            isSuccess = false;
            res = Result.Failure(AllErrors.Category.NotCreated(categoryName));
            mes = MessageBuilder.CategoryExists(categoryName);
        }
        else
        {
            var resCreate = await _categoryService.CreateAsync(state.Value!.UserId, categoryName, parentCategoryName);
            if (resCreate.IsFailure)
            {
                _logger.LogError(resCreate.Error!.ToString());
                res = Result.Failure(resCreate.Error!);
                isSuccess = false;
            }

            _logger.LogInformation(
                $"Пользователь {state.Value.UserId.value} успешно создал категорию \"{categoryName}\"");
        }
        if(isSuccess)
            mes = MessageBuilder.CategoryCreate(categoryName, parentCategoryName);
        
        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: ct);
        return res;
    }

    public async Task<Result> RenameAsync(Update context, string oldCategoryName, string newCategoryName, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());
        
        string mes = "";
        Result res = Result.Success();
        
        var existsOldCategory = await _categoryService.ExistsCategory(state.Value!.UserId, oldCategoryName, ct);
        if (!existsOldCategory)
        {
            mes = MessageBuilder.NotFoundCategory(oldCategoryName);
            res = Result.Failure(AllErrors.Category.NotFound(oldCategoryName));
        }
        else
        {
            var existsNewCategory = await _categoryService.ExistsCategory(state.Value!.UserId, newCategoryName, ct);
            if (existsNewCategory)
            {
                mes = MessageBuilder.CategoryExists(newCategoryName, null);
                res = Result.Failure(AllErrors.Category.NotRenamed(oldCategoryName, newCategoryName));
            }
            else
            {
                res = await _categoryService.RenameAsync(state.Value!.UserId, oldCategoryName, newCategoryName, ct);
                if (res.IsFailure)
                    mes = MessageBuilder.Oops;
                else
                    mes = MessageBuilder.CategoryRenamed(oldCategoryName, newCategoryName);
            }
        }
        
        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: ct);
        return res;
    }

    public async Task<Result> DeleteAsync(Update context, string categoryName, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(context.Message.From.Id, ct);
        if(!state.IsSuccess)
            _logger.LogError(state.Error!.ToString());
        
        string mes = "";
        Result res = default;
        
        var resExistsCategory =  await _fuzzySearch.SearchCategories(context.Message.Chat.Id, categoryName);
        if (resExistsCategory.IsSuccess)
        {
            //TODO: Сделать что-то с операциями этой категории
            if (resExistsCategory.Value.Count() == 1) // Найдена единственная подходящая категория для удаления
            {
                res = await _categoryService.DeleteAsync(state.Value!.UserId, resExistsCategory.Value.First().Id, ct);
                mes = res.IsFailure 
                    ? MessageBuilder.Oops 
                    : MessageBuilder.DeleteCategory(categoryName);
                await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: ct);
                return res;                          
            } else if (resExistsCategory.Value.Count() > 1) // Категорий, подходящих для удаления, найдено несколько
            {
                List<UCIListCategoriesItem> uciCategories = [];
                for(int i = 0; i < resExistsCategory.Value.Count(); i++)
                    uciCategories.Add(new UCIListCategoriesItem(resExistsCategory.Value[i].Id, i+1, resExistsCategory.Value[i].Name));
                var jsonList = JsonSerializer.Serialize(uciCategories);
                res = await _userStateStorage.SetAsync(context.Message.Chat.Id,
                        state.Value
                            .UpdateStatus(UserStatus.WaitEnterCategoryForDelete)
                            .WithContextItem("list_categories", jsonList)
                    );
                mes = MessageBuilder.ListCategoriesForEnter(resExistsCategory.Value!);
                
            }
        }
        else
            res = Result.Failure(AllErrors.Category.NotFound(categoryName));

        if (res.IsFailure)
            mes = MessageBuilder.Oops;
                
        await _bot.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: ct);
        return res;
    }
}
