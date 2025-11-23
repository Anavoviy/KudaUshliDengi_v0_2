using KudaUshliDengi_v0_2.domain.interfaces;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;
using KudaUshliDengi_v0_2.services.result;
using KudaUshliDengi_v0_2.services.result.interfaces;

namespace KudaUshliDengi_v0_2.domain.models;

public class Operation : Entity<OperationId>
{
    public UserId UserId { get; private set; }
    public Money Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateOnly Date { get; private set; }
    public CategoryId? CategoryId { get; private set; } = null;
    public OperationId? ParentId { get; private set; } = null;

    public virtual Category? Category { get; private set; } = null;
    public virtual ICollection<Operation> Items { get; private set; } = [];
    
    private Operation(){}

    // Конструктор простой операции
    public Operation(UserId userId, Money amount, TransactionType type, DateOnly date, CategoryId categoryId)
    {
        Id = OperationId.New();
        UserId = userId;
        Amount = amount;
        Type = type;
        Date = date;
        CategoryId = categoryId;
        
        //AddDomainEvent(new OperationCreatedEvent(this));
    }
    
    // Конструктор составной операции
    public Operation(UserId userId, Money amount, TransactionType type, DateOnly date)
    {
        Id = OperationId.New();
        UserId = userId;
        Amount = amount;
        Type = type;
        Date = date;
    }

    public void Cancel()
    {
        this.Delete();
        //AddDomainEvent(new OperationCancelledEvent(this));
    }

    public void AddItem(Money amount, CategoryId categoryId)
        => Items.Add(new Operation(UserId, amount, Type, Date, categoryId){ParentId = this.Id});

    public IResult ValidateTotalAmount()
    {
        decimal sum = Items.Sum(x => x.Amount);
        return sum != Amount
            ? Result.Failure(Error.New($"Сумма чеков ({Amount}) != сумме составной операции ({sum})", 1)) //TODO: Ввести систему кодов
            : Result.Success();;
    }
}

public enum TransactionType
{
    Income,
    Expense
}

public record OperationData(Money Amount, string categoryName);