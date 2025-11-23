using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.services.result.errors;

public class CategoryErrors
{
    //200
    
    //201
    public IError NotFound(string categoryName)
        => Error.New($"Не удалось найти категорию \"{categoryName}\"", 201);
    //202
    
    //203
    
    //204
    public IError NotCreated(string categoryName)
        => Error.New($"Не удалось создать категорию {categoryName}!", 204);

    //205
    public IError NotRenamed(string oldCategoryName, string newCategoryName)
        => Error.New($"Не удалось переименовать категорию {oldCategoryName} в {newCategoryName}! Возможно существует категория с таким названием!", 205);
}