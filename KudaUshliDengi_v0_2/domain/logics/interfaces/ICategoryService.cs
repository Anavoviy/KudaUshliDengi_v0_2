using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface ICategoryService
{
    /// <summary>
    /// Добавление новой категории
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="categoryName"></param>
    /// <param name="parentCategoryName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> CreateAsync(UserId userId, string categoryName, string? parentCategoryName = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление категории 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> DeleteAsync(UserId userId, CategoryId categoryId, CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Получение всех категорий пользователя </summary>
    /// <param name="userId"></param>
    /// <param name="categoryName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<List<Category>>> GetAsync(UserId userId, bool grouping = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создаёт базовый набор категорий для пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Result, указывающий на успешность операции</returns>
    Task<Result> CreateBasicCategoriesForUser(UserId userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsIncomeCategory(UserId userId, string nameNew, CancellationToken cancellationToken = default);
    Task<bool> ExistsCategory(UserId userId, string nameNew, CancellationToken cancellationToken = default);
    Task<Result> RenameAsync(UserId userId, string oldCategoryName, string newCategoryName, CancellationToken cancellationToken = default);
}