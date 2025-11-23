using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.infrastructure.fuzzy_search;
using KudaUshliDengi_v0_2.infrastructure.storages.interfaces;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;

namespace KudaUshliDengi_v0_2.application.logic.services;

public class FuzzySearchService : IFuzzySearchService
{
    private FuzzySearcher _searcher;
    private readonly IStateStorage<long, UserState> _storage;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly IGoalService _goalService;

    public FuzzySearchService(IStateStorage<long,  UserState> storage, ICategoryService categoryService, IGoalService goalService)
    {
        _searcher = new FuzzySearcher(0.3f);
        _storage = storage;
        
        _categoryService = categoryService;
        _goalService = goalService;
    }

    public async Task<Result<List<Category>>> SearchCategories(long userId, string targetCategory, bool income = false)
    {
        var state = await _storage.GetAsync(userId);
        var categories = await _categoryService.GetAsync(state.Value.UserId);

        var incomeCategory = categories.Value.FirstOrDefault(c => c.Name == "доходы");
        if (income)
            categories.Value.RemoveAll(c => c.ParentId != incomeCategory.Id);
        else
            categories.Value.RemoveAll(c => c.Id == incomeCategory.Id || c.ParentId == incomeCategory.Id);        
        
        if (!categories.IsSuccess)
            return categories;

        var searched = _searcher.SearchMany(targetCategory, categories.Value.Select(c => c.Name));
        if(!searched.Any())
            return Result<List<Category>>.NotFound(Error.New("Не удалось найти подходящие категории", 131));

        List<Category> res = new();
        foreach (var name in searched)
            res.Add(categories.Value.FirstOrDefault(c => c.Name == name));
        
        return Result<List<Category>>.Success(res);
    }

    public async Task<Result<List<Goal>>> SearchGoals(long userId, string targetGoal)
    {
        var state = await _storage.GetAsync(userId);
        var goals = await _goalService.GetAllAsync(state.Value.UserId);

        if (!goals.IsSuccess)
            return goals;

        var searched = _searcher.SearchMany(targetGoal, goals.Value.Select(c => c.Name));
        if(!searched.Any())
            return Result<List<Goal>>.NotFound(AllErrors.Goal.NotFound(targetGoal));

        List<Goal> res = new();
        foreach (var name in searched)
            res.Add(goals.Value.FirstOrDefault(c => c.Name == name));
        
        return Result<List<Goal>>.Success(res);
    }
}