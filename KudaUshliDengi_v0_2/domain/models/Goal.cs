using KudaUshliDengi_v0_2.domain.interfaces;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.domain.models;

public class Goal : Entity<GoalId>
{
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public Money TargetAmount { get; private set; }
    public Money CurrentAmount { get; private set; }
    public bool IsCompleted { get; private set; } = false;

    public virtual User? User { get; set; } = null!;

    private Goal() : base(){}

    public Goal(UserId userId, string name, Money? targetAmount)
    {
        Id = GoalId.New();
        UserId = userId;
        Name = name;
        TargetAmount = targetAmount ?? Money.Empty;
    }

    public void UpdateTargetAmount(Money targetAmount) => TargetAmount = targetAmount;

    public void IncreaseAmount(Money amount)
    {
        CurrentAmount += amount;
        
        if(TargetAmount != Money.Empty && CurrentAmount >= TargetAmount && !IsCompleted)
            Complete();
    }

    public IResult DecreaseAmount(Money amount)
    {
        if (CurrentAmount >= amount)
            CurrentAmount -= amount;

        return Result.Success(); //TODO: Проверь типы!
    }

    public void Complete()
    {
        if (IsCompleted) return;
        
        if (TargetAmount == CurrentAmount)
            IsCompleted = true;
        
        //AddDomainEvent(new GoalCompletedEvent(this));
    }
}