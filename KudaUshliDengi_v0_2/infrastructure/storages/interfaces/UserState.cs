using System.Collections.Concurrent;
using KudaUshliDengi_v0_2.domain.valueobjects.ids;

namespace KudaUshliDengi_v0_2.infrastructure.storages.interfaces;

public record UserState
{
    public UserState(UserId userId, UserStatus Status, ConcurrentDictionary<string, string>? Context = null)
    {
        this.UserId = userId;
        this.Status = Status;
        this.Context = Context ?? new ConcurrentDictionary<string, string>();
    }
    public UserId UserId { get; init; } 
    public UserStatus Status { get; init; }
    public ConcurrentDictionary<string, string> Context { get; init; }
}

public static class UserStateExtensions
{
    public static UserState UpdateStatus(this UserState state, UserStatus status)
        => new UserState(state.UserId, status, state.Context);
    
    public static UserState WithContextItem(this UserState state, string key, string value)
    {
        var newContext = new ConcurrentDictionary<string, string>(state.Context);
        newContext[key] = value;
        return new UserState(state.UserId, state.Status, newContext);
    }

    public static UserState WithContextItems(this UserState state, params (string key, string value)[] items)
    {
        var newContext = new ConcurrentDictionary<string, string>(state.Context);
        foreach (var (key, value) in items)
            newContext[key] = value;
        return new UserState(state.UserId, state.Status, newContext);
    }

    public static UserState WithoutContextItem(this UserState state, string key)
    {
        var newContext = new ConcurrentDictionary<string, string>(state.Context);
        newContext.TryRemove(key, out _);
        return new UserState(state.UserId, state.Status, newContext);
    }

    public static UserState WithoutContextItems(this UserState state, params string[] keys)
    {
        var newContext = new ConcurrentDictionary<string, string>(state.Context);
        foreach (var key in keys)
            newContext.TryRemove(key, out _);
        return new UserState(state.UserId, state.Status, newContext);
    }

    public static UserState WithClearedContext(this UserState state)
        => new UserState(state.UserId, state.Status);
}