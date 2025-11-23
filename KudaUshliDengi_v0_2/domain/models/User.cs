using KudaUshliDengi_v0_2.domain.events.user;
using KudaUshliDengi_v0_2.domain.interfaces;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.domain.models;

public class User : Entity<UserId>
{
    public long TgUserId { get; private set; }
    public long TgChatId { get; private set; }
    public string TgUsername { get; private set; }
    
    //public PhoneNumber PhoneNumber { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = [];

    private User(){}
    
    public User(long tgUserId, long tgChatId, string? tgUsername)
    {
        Id = UserId.New();
        TgUserId = tgUserId;
        TgChatId = tgChatId;
        TgUsername = tgUsername ?? String.Empty;
        
        AddDomainEvent(new RegisterNewUser(this));
    }
}