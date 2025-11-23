using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.domain.events.category;

public record CategoryRenamed : DomainEvent
{
    public UserId UserId {get; private set;}
    public CategoryId CategoryId {get; private set;}
    public string OldName { get; private set; }
    public string NewName { get; private set; }

    public CategoryRenamed(Category category, string oldName)
    {
        UserId = category.UserId;
        CategoryId = category.Id;
        OldName = oldName;
        NewName = category.Name;
    }
}