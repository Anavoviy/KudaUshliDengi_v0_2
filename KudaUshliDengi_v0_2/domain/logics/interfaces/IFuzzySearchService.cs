using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.services.result;

namespace KudaUshliDengi_v0_2.domain.logics.interfaces;

public interface IFuzzySearchService
{
    Task<Result<List<Category>>> SearchCategories(long userId, string targetCategory, bool income = false);
    Task<Result<List<Goal>>> SearchGoals(long userId, string targetGoal);
}