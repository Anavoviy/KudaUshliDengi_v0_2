using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.infrastructure.storages.context_items;

public struct UCIOperationData
{
    public UserId UserId { get; set; }
    public TransactionType Type { get; set; }
    public Money Amount { get; set; }
    public DateOnly Date { get; set; }

    public UCIOperationData(){}

    public UCIOperationData(UserId userId, TransactionType type, Money amount, DateOnly date)
    {
        UserId = userId;
        Amount = amount;
        Date = date;
        Type = type;
    }
}