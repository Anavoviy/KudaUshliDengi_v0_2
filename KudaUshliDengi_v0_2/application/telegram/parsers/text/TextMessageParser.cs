using System.Text.Json;
using System.Text.RegularExpressions;
using KudaUshliDengi_v0_2.application.telegram.builders;
using KudaUshliDengi_v0_2.application.telegram.interfaces;
using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.infrastructure.storages.context_items;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace KudaUshliDengi_v0_2.application.telegram.parsers.text;

public class TextMessageParser : IMessageParser
{
    public MessageType MessageType => MessageType.Text;
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly IGoalService _goalService;
    private readonly IOperationService _operationService;
    private readonly IFuzzySearchService _fuzzySearch;
    private readonly IStateStorage<long, UserState>  _userStateStorage;
    
    public TextMessageParser(
        ITelegramBotClient botClient, 
        IUserService userService, 
        ICategoryService categoryService,
        IOperationService operationService,
        IFuzzySearchService fuzzySearch,
        IStateStorage<long, UserState> userStateStorage)
    {
        _botClient = botClient;

        _userService = userService;
        _categoryService = categoryService;
        _operationService  = operationService;
        
        _userStateStorage = userStateStorage;
        _fuzzySearch = fuzzySearch;
    }
    
    public async Task<Result> ParseAsync(Message msg, CancellationToken ct = default, Update? context = null)
    {
        if(msg.Text == string.Empty)
            return Result.Failure(BaseError.New("Текст пустой... как так", 99)); //TODO: Придумай ты уже коды ошибок

        string text = msg.Text!.Trim();

        if (text == "/start")
        {
            await HandleStartCommand(msg, ct);
            return Result.Success();
        }
        if (text == "Помощь"){
            await HandleHelp(msg, ct);
            return Result.Success();
        }
        if(text == "Назад"){
            await HandleBack(msg, ct);
            return Result.Success();
        }
        
        UserStatus userStatus = (await _userStateStorage.GetAsync(msg.From.Id)).Value.Status;
        var resInvoke = userStatus switch
        {
            UserStatus.Main => await HandleMainCommand(text, context, ct),
            UserStatus.Categories => await HandleCategoriesCommand(text, context, ct),
            UserStatus.WaitEnterCategoryForDelete => await HandleManuCategoriesDelete(text, context, ct),
        };

        return resInvoke;
    }

    private async Task<Result> HandleManuCategoriesDelete(string text, Update? context, CancellationToken ct)
    {
        string numStr = "";
        Regex.Matches(text, @"\d").ToList().ForEach(m => numStr += m.Value.ToString());
        int num = int.Parse(numStr);

        Result res;
        string mes;
        
        var state = await _userStateStorage.GetAsync(context.Message.From.Id);
        if (state.IsSuccess)
        {
            var listUCIcategories = JsonSerializer.Deserialize<List<UCIListCategoriesItem>>(state.Value.Context["list_categories"]);
            if (listUCIcategories.Count > 0 && num <= listUCIcategories.Count)
            {
                var selectCategory =  listUCIcategories.FirstOrDefault(c => c.Number == num);
                
                var result = await _categoryService.DeleteAsync(state.Value.UserId, selectCategory.CategoryId, ct);
                if(result.IsSuccess){
                    mes = MessageBuilder.DeleteCategory(selectCategory.Name);
                    res = await _userStateStorage.SetAsync(context.Message.From!.Id, state.Value.UpdateStatus(UserStatus.Categories));
                }
                else
                {
                    res = Result.Failure(BaseError.New("Не удалось удалить категорию!", 510));
                    mes = MessageBuilder.Oops;    
                }
            }
            else
            {
                res = Result.Failure(BaseError.New("Введённый номер не входит в список номеров категорий!", 454));
                mes = MessageBuilder.Oops;
            }
        }
        else
        {
            mes = MessageBuilder.Oops;
            res = Result.Failure(state.Error!);
        }

        await _botClient.SendMessage(context.Message.Chat.Id, mes, ParseMode.Markdown, cancellationToken: ct);
        return res;
    }

    private async Task<Result> HandleCategoriesCommand(string text, Update? context, CancellationToken ct)
    {
        var userState = await _userStateStorage.GetAsync(context.Message.From.Id);
        if(!userState.IsSuccess)
            return Result.Failure(userState.Error!);
        var userId = userState.Value.UserId;

        string msg = "";
        
        switch (text)
        {
            case "Все категории": // Вывод списка всех категорий пользователя
                var categories = await _categoryService.GetAsync(userId, true);
                if (categories.IsNotFound)
                    await _botClient.SendMessage(context.Message.Chat.Id, "У вас отсутствуют категории🥹",
                        cancellationToken: ct);
                else
                {
                    msg = MessageBuilder.ListCategories(categories.Value);
                    await _botClient.SendMessage(context.Message.Chat.Id, msg, parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
                break;
            default: // Обработка команд с данными
                string[] msgWords = text.Trim().Split(' ');
                if(msgWords.Length <= 1)
                    return Result.Failure(BaseError.New("Ничего не передано!", 440));
                if (msgWords[0].ToLower() == "новый" && msgWords.Length >= 2) // Создание новой категории
                {
                    if (msgWords[1].ToLower() == "доход") // Создание новой категории дохода
                    {
                        string nameNew = string.Join(" ", msgWords[2..]);
                        if (await _categoryService.ExistsIncomeCategory(userId, nameNew, ct)) // Отработка того, что категория уже существует
                        {
                            msg = MessageBuilder.CategoryExists(nameNew, true);
                            await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                            return Result.Failure(BaseError.New("Категория с таким названием уже существует!", 435));
                        }

                        var resCreate = await _categoryService.CreateAsync(userId, nameNew, "доходы", ct);
                        
                        msg = resCreate.IsFailure ? MessageBuilder.Oops : MessageBuilder.CategoryCreate(nameNew, "доходы");
                        
                        await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                        return resCreate;
                    }
                    else // Создание новой категории расхода
                    {
                        string nameNew = string.Join(" ", msgWords[1..]);
                        
                        Result resCreate;
                        string nameParent = null;
                        if (msgWords.Contains("в")) // Создание подкатегории расхода
                        {
                            int indexIn = msgWords.ToList().IndexOf("в");
                            nameNew = string.Join(" ",  msgWords[1..indexIn]);
                            
                            if (await _categoryService.ExistsExpenseCategory(userId, nameNew, ct)) // Отработка того, что категория уже существует
                            {
                                msg = MessageBuilder.CategoryExists(nameNew);
                                await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                                return Result.Failure(BaseError.New("Категория с таким названием уже существует!", 435));
                            }
                            
                            nameParent = string.Join(" ", msgWords[(indexIn+1)..]);
                            var resExistsParent =  await _fuzzySearch.SearchCategories(context.Message.Chat.Id, nameParent);
                            if (resExistsParent.IsSuccess && resExistsParent.Value.Count() == 1)
                            {
                                nameParent = resExistsParent.Value.First().Name;
                                resCreate = await _categoryService.CreateAsync(userId, nameNew,
                                    nameParent, cancellationToken: ct);
                            }
                            else 
                                resCreate = Result.Failure(resExistsParent.Error!);
                        }
                        else
                        {
                            if (await _categoryService.ExistsExpenseCategory(userId, nameNew, ct)) // Отработка того, что категория уже существует
                            {
                                msg = MessageBuilder.CategoryExists(nameNew);
                                await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                                return Result.Failure(BaseError.New("Категория с таким названием уже существует!", 435));
                            }
                            
                            resCreate = await _categoryService.CreateAsync(userId, nameNew, cancellationToken: ct);
                        }

                        msg = resCreate.IsFailure ? MessageBuilder.Oops : MessageBuilder.CategoryCreate(nameNew, nameParent);
                        
                        await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                        return resCreate;
                    }
                } else if (msgWords[0] == "x" || msgWords[0] == "х" || msgWords[0] == "×" )
                {
                    string nameCategory = string.Join(" ", msgWords[1..]);
                    var resExistsCategory =  await _fuzzySearch.SearchCategories(context.Message.Chat.Id, nameCategory);
                    if (resExistsCategory.IsSuccess)
                    {
                        if (resExistsCategory.Value.Count() == 1) // Найдена единственная подходящая категория для удаления
                        {
                            var res = await _categoryService.DeleteAsync(userId, resExistsCategory.Value.First().Id, ct);
                            msg = res.IsFailure 
                                ? MessageBuilder.Oops 
                                : MessageBuilder.DeleteCategory(nameCategory);
                            await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                            return res;                            
                        } else if (resExistsCategory.Value.Count() > 1) // Категорий, подходящих для удаления, найдено несколько
                        {
                            List<UCIListCategoriesItem> uciCategories = [];
                            for(int i = 0; i < resExistsCategory.Value.Count(); i++)
                                uciCategories.Add(new UCIListCategoriesItem(resExistsCategory.Value[i].Id, i+1, resExistsCategory.Value[i].Name));
                            var jsonList = JsonSerializer.Serialize(uciCategories);
                            var resUpdate = await _userStateStorage.SetAsync(context.Message.Chat.Id,
                                    userState.Value
                                        .UpdateStatus(UserStatus.WaitEnterCategoryForDelete)
                                        .WithContextItem("list_categories", jsonList)
                                );
                            msg = MessageBuilder.ListCategoriesForEnter(resExistsCategory.Value!);
                            if (resUpdate.IsFailure)
                                msg = MessageBuilder.Oops;
                            
                            await _botClient.SendMessage(context.Message.Chat.Id, msg, ParseMode.Markdown, cancellationToken: ct);
                            return resUpdate;
                        }
                    }
                }
                    
                break;
        }
        
        return Result.Success();
    }

    private async Task HandleStartCommand(Message msg, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(msg.From.Id);
        await _userStateStorage.SetAsync(msg.From!.Id, state.Value.UpdateStatus(UserStatus.Main)); //Ставим меню в положение Main
        await _botClient.SendMessage(msg.Chat.Id, "Вы вернулись в главном меню", replyMarkup: KeyboardBuilder.Main, cancellationToken: ct);
    }
    private async Task HandleBack(Message msg, CancellationToken ct)
    {
        var state = await _userStateStorage.GetAsync(msg.From.Id);
        UserStatus status;
        string mes;
        
        switch (state.Value.Status)
        {
            case UserStatus.WaitEnterCategoryForDelete:
                status = UserStatus.Categories;
                mes = "Вы вернулись в меню категорий";
                break;
            default:
                status = UserStatus.Main;
                mes = "Вы вернулись в главном меню";
                break;
        }

        await _userStateStorage.SetAsync(msg.From!.Id, state.Value.UpdateStatus(status));
        await _botClient.SendMessage(msg.Chat.Id, mes, replyMarkup: KeyboardBuilder.Main, cancellationToken: ct);
    }
    private async Task HandleHelp(Message msg, CancellationToken ct = default)
        => await _botClient.SendMessage(msg.Chat.Id, MessageBuilder.Help, ParseMode.Markdown, cancellationToken: ct);

    private async Task<Result> HandleMainCommand(string command, Update context, CancellationToken ct)
    {
        bool isMenu = false;
        var state = await _userStateStorage.GetAsync(context.Message.From.Id);
        switch (command)
        {
            case "Цели":
                await _userStateStorage.SetAsync(context.Message!.From!.Id, state.Value.UpdateStatus(UserStatus.Goals)); //Ставим меню в положение Goals
                await _botClient.SendMessage(context.Message!.Chat.Id, "Вы перешли в меню целей", replyMarkup: KeyboardBuilder.Goals, cancellationToken: ct);
                isMenu = true;
                break;
            case "Категории":
                await _userStateStorage.SetAsync(context.Message!.From!.Id, state.Value.UpdateStatus(UserStatus.Categories)); //Ставим меню в положение Categories
                await _botClient.SendMessage(context.Message!.Chat.Id, "Вы перешли в меню категорий", replyMarkup: KeyboardBuilder.Categories, cancellationToken: ct);
                isMenu = true;
                break;
            case "Лимиты":
                await _userStateStorage.SetAsync(context.Message!.From!.Id, state.Value.UpdateStatus(UserStatus.Limits)); //Ставим меню в положение Limits
                await _botClient.SendMessage(context.Message!.Chat.Id, "Вы перешли в меню лимитов", replyMarkup: KeyboardBuilder.Limits, cancellationToken: ct);
                isMenu = true;
                break;
            case "Отчеты":
                await _userStateStorage.SetAsync(context.Message!.From!.Id, state.Value.UpdateStatus(UserStatus.Reports)); //Ставим меню в положение Reports
                await _botClient.SendMessage(context.Message!.Chat.Id, "Вы перешли в меню отчётов", replyMarkup: KeyboardBuilder.Reports, cancellationToken: ct);
                isMenu = true;
                break;
                        
        }
        if(isMenu) return Result.Success();

        if (command[0] == '+')
        {
            //TODO: Парсинг дохода
            string[] strs = command.Substring(1).Split(' ');
            if(!decimal.TryParse(strs[0], out var amount))
                return Result.Failure(BaseError.New("При создании дохода сумма указана не в формате числа!", 412));
            
            string categoryName = string.Join(" ", strs[1..]);
            var searchCategories = await _fuzzySearch.SearchCategories(context.Message.From!.Id, categoryName, true);
            if (searchCategories.IsFailure)
                return Result.Failure(searchCategories.Error);
            
            if(searchCategories.IsNotFound || searchCategories.Value.Count <= 0) //TODO: Можно добавить поиск среди категорий расходов, чтобы дать подсказку, что категория не из доходов, а из расходов
                await _botClient.SendMessage(context.Message.Chat.Id, MessageBuilder.NotFoundCategory(categoryName, TransactionType.Income), ParseMode.Markdown, cancellationToken: ct);
            
            if (searchCategories.Value.Count == 1)
            {
                var operation = await _operationService.CreateIncomeAsync(state.Value.UserId,new Money(amount), searchCategories.Value.First().Id);
                if(!operation.IsSuccess)
                    return Result.Failure(operation.Error);

                var mes = MessageBuilder.CreateOperation(operation.Value, searchCategories.Value.First().Name);
                
                await _botClient.SendMessage(context.Message.Chat.Id, 
                    mes, 
                    ParseMode.Markdown,
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendMessage(context.Message.Chat.Id,
                    MessageBuilder.ListCategoriesForEnter(searchCategories.Value), 
                    cancellationToken: ct);
                List<UCIListCategoriesItem> listCategories = new();
                for (int i = 0; i < searchCategories.Value.Count; i++)
                    listCategories.Add(new UCIListCategoriesItem(searchCategories.Value[i].Id, i+1, searchCategories.Value[i].Name));
                string jsonListCategories = JsonSerializer.Serialize(listCategories);

                UCIOperationData operationData = new UCIOperationData(state.Value.UserId, TransactionType.Income, new Money(amount), DateOnly.FromDateTime(DateTime.UtcNow));
                string jsonOperationData = JsonSerializer.Serialize(operationData);
                
                await _userStateStorage.SetAsync(
                    context.Message!.From!.Id, 
                    state.Value.UpdateStatus(UserStatus.WaitEnterCategoryForCreateOperation)
                        .WithContextItems(
                            ("list_categories", jsonListCategories), 
                            ("operation_data", jsonOperationData)
                        ));
                await _botClient.SendMessage(context.Message.Chat.Id, 
                    MessageBuilder.ListCategoriesForEnter(searchCategories.Value), 
                    cancellationToken: ct);
            }
        }
        else if (command.Contains("на") || command.Contains("из"))
        {
            //TODO: Парсинг операций с копилками
        }
        else if(command.IndexOf(':') > -1)
        {
            //TODO: Парсинг сложного расхода
        }
        else
        {
            string[] strs = command.Split(' ');
            if(!decimal.TryParse(strs[0], out var amount))
                return Result.Failure(BaseError.New("При создании дохода сумма указана не в формате числа!", 412));
            
            string categoryName = string.Join(" ", strs[1..]);
            var searchCategories = await _fuzzySearch.SearchCategories(context.Message.From!.Id, categoryName);
            if (searchCategories.IsFailure)
                return Result.Failure(searchCategories.Error);
            
            if(searchCategories.IsNotFound || searchCategories.Value.Count <= 0) //TODO: Можно добавить поиск среди категорий доходов, чтобы дать подсказку, что категория не из расходов, а из доходов
                await _botClient.SendMessage(context.Message.Chat.Id, MessageBuilder.NotFoundCategory(categoryName, TransactionType.Expense), ParseMode.Markdown, cancellationToken: ct);
            
            if (searchCategories.Value.Count == 1)
            {
                var operation = await _operationService.CreateExpenseAsync(state.Value.UserId,new Money(amount), searchCategories.Value.First().Id, ct);
                if(!operation.IsSuccess)
                    return Result.Failure(operation.Error);

                var mes = MessageBuilder.CreateOperation(operation.Value, searchCategories.Value.First().Name);
                
                await _botClient.SendMessage(context.Message.Chat.Id, 
                    mes, 
                    ParseMode.Markdown,
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendMessage(context.Message.Chat.Id,
                    MessageBuilder.ListCategoriesForEnter(searchCategories.Value), 
                    cancellationToken: ct);
                List<UCIListCategoriesItem> listCategories = new();
                for (int i = 0; i < searchCategories.Value.Count; i++)
                    listCategories.Add(new UCIListCategoriesItem(searchCategories.Value[i].Id, i+1, searchCategories.Value[i].Name));
                string jsonListCategories = JsonSerializer.Serialize(listCategories);

                UCIOperationData operationData = new UCIOperationData(state.Value.UserId, TransactionType.Expense, new Money(amount), DateOnly.FromDateTime(DateTime.UtcNow));
                string jsonOperationData = JsonSerializer.Serialize(operationData);
                
                await _userStateStorage.SetAsync(
                    context.Message!.From!.Id, 
                    state.Value.UpdateStatus(UserStatus.WaitEnterCategoryForCreateOperation)
                        .WithContextItems(
                            ("list_categories", jsonListCategories), 
                            ("operation_data", jsonOperationData)
                        ));
                await _botClient.SendMessage(context.Message.Chat.Id, 
                    MessageBuilder.ListCategoriesForEnter(searchCategories.Value), 
                    cancellationToken: ct);
            }
        }
            
        return Result.Success();
    }
}