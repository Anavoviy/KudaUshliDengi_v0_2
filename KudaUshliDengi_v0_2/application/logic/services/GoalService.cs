using KudaUshliDengi_v0_2.domain.logics.interfaces;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.infrastructure.ef_core.context;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.errors;
using KudaUshliDengi_v0_2.services.try_catcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KudaUshliDengi_v0_2.application.logic.services;

public class GoalService(
    SqliteDbContext context,
    
    ILogger<GoalService> _logger
    ) : IGoalService
{
    public async Task<Result<List<Goal>>> GetAllAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var goals = await  context.Goals.Where(g => g.UserId == userId && g.DeletedAt == null).ToListAsync();
        
        if (!goals.Any())
            return Result<List<Goal>>.NotFound(AllErrors.Goal.NotFound());
        
        return Result<List<Goal>>.Success(goals);
    }
    
    public async Task<Result<Goal>> CreateAsync(UserId userId, string name, Money targetAmount, CancellationToken cancellationToken = default)
    {
        var exists = await context.Goals.AnyAsync(
            g => 
                g.UserId == userId && 
                g.Name == name && 
                g.DeletedAt == null,
            cancellationToken);

        if (exists)
            return Result<Goal>.Failure(AllErrors.Goal.Exists(name));

        return await TCatcher.Async(async () =>
        {
            Goal goal = new Goal(userId, name, targetAmount);
            await context.Goals.AddAsync(goal, cancellationToken);
            await context.CommitAsync(cancellationToken);
            
            return goal;
        }, cancellationToken);
    }

    public Task<Result> IncomeAsync(UserId userId, string name, Money amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ExpenseAsync(UserId userId, string name, Money amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> CancelAsync(UserId userId, string name, CancellationToken cancellationToken = default)
    {
        var goal = await context.Goals.FirstOrDefaultAsync(g => g.UserId == userId && g.DeletedAt == null && g.IsCompleted == false);
        if (goal is null)
            return Result.Failure(AllErrors.Goal.NotFound(name));
        
        goal.Complete();

        return await TCatcher.Async(async () =>
        {
            context.Goals.Update(goal);
            await context.CommitAsync(cancellationToken);
        });
    }

    public async Task<Result> RenameAsync(UserId userId, string name, string newName, CancellationToken cancellationToken = default)
    {
        Goal? goal = await context.Goals.FirstOrDefaultAsync(g => g.UserId == userId && g.DeletedAt == null && g.Name == name);
        if (goal is null)
            return Result.Failure(AllErrors.Goal.NotFound(name));
        
        return await TCatcher.Async(async () =>
        {
            goal.Rename(newName);
            context.Goals.Update(goal);
            await context.CommitAsync(cancellationToken);
        });
    }
}