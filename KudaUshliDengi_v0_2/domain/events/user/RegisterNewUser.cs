using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.domain.events.user;

public record RegisterNewUser : DomainEvent
{
    public UserId UserId { get; init; }
    public string Username { get; init; }
    public long TgChatId { get; init; }
    public long TgUserId  { get; init; }
    
    
    public RegisterNewUser(User user)
    {
        UserId = user.Id;
        Username = user.TgUsername;
        TgChatId = user.TgChatId;
        TgUserId = user.TgUserId;
    }
}