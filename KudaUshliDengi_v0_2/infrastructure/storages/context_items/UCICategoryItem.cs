using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.infrastructure.storages.context_items;

public struct UCIListCategoriesItem
{
    public CategoryId CategoryId { get; set; }
    public int Number  { get; set; }
    public string Name { get; set; }

    public UCIListCategoriesItem()
    {
    }

    public UCIListCategoriesItem(CategoryId categoryId, int number,  string name)
    {
        CategoryId = categoryId;
        Number = number;
        Name = name;
    }
}