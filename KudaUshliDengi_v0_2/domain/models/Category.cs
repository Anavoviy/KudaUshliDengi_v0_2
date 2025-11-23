using KudaUshliDengi_v0_2.domain.events.category;
using KudaUshliDengi_v0_2.domain.interfaces;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.domain.models;

public class Category : Entity<CategoryId>
{
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public CategoryId? ParentId { get; private set; }
    
    public virtual Category? ParentCategory { get; private set; } = null!;
    public virtual ICollection<Category> Childrens { get; private set; } = [];

    private Category(){}

    public Category(UserId userId, string name, CategoryId? parentId = null)
    {
        Id = CategoryId.New();
        UserId = userId;
        Name = name;
        ParentId = parentId;
    }

    public void Rename(string newCategoryName)
    {
        string oldCategoryName = Name;
        this.Name = newCategoryName;
        AddDomainEvent(new CategoryRenamed(this, oldCategoryName));
    }
}