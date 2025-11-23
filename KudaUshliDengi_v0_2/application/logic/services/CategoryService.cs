using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using KudaUshliDengi_v0_2.services.try_catcher;
using Microsoft.EntityFrameworkCore;

namespace KudaUshliDengi_v0_2.application.logic.services;

public class CategoryService(SqliteDbContext context) : ICategoryService
{
    private readonly List<string[]> BasicCategoriesNames
        = [
            ["доходы", "зарплата", "премия", "фриланс", "инвестиции", "вклады", "подарки", "долг", "налоги"],
            ["продукты", "вкусняшки", "доставка"],
            ["одежда", "обувь", "аксессуары"],
            ["электроника", "комплектующие", "смарт"],
            ["красота", "косметика", "парфюм", "уход"],
            ["дом", "мебель", "хозтовары", "аренда", "жкх", "ипотека", "ремонт"],
            ["транспорт", "автобус", "такси", "топливо", "страховка", "то", "авторемонт", "аренда"],
            ["развлечения", "кафе", "рестораны", "культура", "хобби", "отдых"],
            ["здоровье", "врачи", "анализы", "спорт", "витамины", "лекарства"],
            ["образование", "учебники", "репетиторы", "консультации"],
            ["финансы", "налоги", "банк", "кредиты"],
            ["личное", "подарки", "благотворительность", "долг"],
            ["путешествие", "перелет", "проживание", "питание", "досуг"],
            ["связь", "телефон", "интернет", "подписки"],
        ];  
    
    public async Task<Result> CreateAsync(UserId userId, string categoryName, string? parentCategoryName = null, CancellationToken cancellationToken = default)
    {
        CategoryId? parentId = (await context.Categories.Select(c => new { c.Id, c.UserId, c.Name })
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == parentCategoryName))?.Id;

        if (parentCategoryName is not null && parentId is null)
            return Result.Failure(Error.New($"Не удалось найти основную категорию: {parentCategoryName}", 439)); //TODO: Поменять ошибки

        Category newCategory = parentCategoryName is null
            ? new Category(userId, categoryName)
            : new Category(userId, categoryName, parentId);
        
        return await TCatcher.Async(async () =>
        {
            await context.Categories.AddAsync(newCategory);
            await context.CommitAsync();
        });
    }

    public async Task<Result> DeleteAsync(UserId userId, CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        await context.Categories.Where(c => c.Id == categoryId && c.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await context.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<Category>>> GetAsync(UserId userId, bool grouping = false, CancellationToken cancellationToken = default)
    {
        var categories = grouping 
            ? await context.Categories.Include(c => c.Childrens).Where(c => c.UserId == userId && c.ParentId == null).OrderBy(c => c.Name).ToListAsync() 
            : await context.Categories.Where(c => c.UserId == userId).OrderBy(c => c.Name).ToListAsync();

        if (categories.Count == 0)
            return Result<List<Category>>.NotFound(Error.New("У пользователя отсутствуют категории!", 130)); //TODO: Поменять ошибки
        
        return Result<List<Category>>.Success(categories);
    }

    public async Task<Result> CreateBasicCategoriesForUser(UserId userId, CancellationToken cancellationToken = default)
    {
        var categories = BasicCategories(userId);
        return await TCatcher.Async(async () =>
        {
            await context.Categories.AddRangeAsync(categories);
            await context.CommitAsync();
        }, cancellationToken);
    }

    public async Task<bool> ExistsIncomeCategory(UserId userId, string nameNew, CancellationToken cancellationToken = default)
    {
        var parentIncomeId = (await context.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "доходы")).Id;
        return await context.Categories.AnyAsync(c => c.Name == nameNew && c.ParentId == parentIncomeId);
    }
    public async Task<bool> ExistsCategory(UserId userId, string nameNew, CancellationToken cancellationToken = default)
        => await context.Categories.AnyAsync(c => c.UserId == userId && c.Name == nameNew);

    public async Task<Result> RenameAsync(UserId userId, string oldCategoryName, string newCategoryName,
        CancellationToken cancellationToken = default)
        => await TCatcher.Async(async () =>
        {
            var category =  await context.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == oldCategoryName);
            category?.Rename(newCategoryName);
            await context.CommitAsync();
        });

    private List<Category> BasicCategories(UserId userId)
    {
        List<Category> categories = new();
        foreach (var categoryGroup in BasicCategoriesNames)
        {
            Category parentCategory = new Category(userId, categoryGroup[0]);
            categories.Add(parentCategory);
            if (categoryGroup.Length > 1)
            {
                for(int i = 1; i < categoryGroup.Length; i++)
                    categories.Add(new Category(userId, categoryGroup[i], parentCategory.Id));
            }
        }
        
        return categories;
    }
}