namespace KudaUshliDengi_v0_2.domain.valueobjects;

public record Money
{
    public decimal Amount { get; init; }

    public Money(decimal amount)
    {
        if(amount < 0)
            throw new ArgumentException($"Amount can't be zero or less zero! Amount = {amount}");
        
        Amount = amount;
    }
    
    public static Money Empty => new Money(0);
    
    public static Money operator +(Money a, Money b)
        => new Money(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b)
    {
        decimal amount = a.Amount - b.Amount;
        return amount < 0 
            ? throw new ArgumentException($"Amount can't be zero or less zero! Amount = {amount}") 
            : new Money(amount);
    }

    public virtual bool Equals(Money? other) 
        => other is not null && this.Amount == other.Amount;
    public override int GetHashCode() 
        => this.Amount.GetHashCode();

    public override string ToString()
        => $"{Amount.ToString("F2")} руб";

    public static implicit operator decimal(Money money) 
        => money.Amount;
    public static explicit operator Money(decimal amount) 
        => new Money(amount);
}