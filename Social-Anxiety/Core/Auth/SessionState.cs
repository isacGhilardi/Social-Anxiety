namespace Social_Anxiety.Core.Auth;

/// <summary>Sessão atual do app: convidado, usuário logado ou nenhuma.</summary>
public sealed class SessionState(ISessionStore store, IUserRepository users)
{
    internal const string GuestMarker = "guest";

    public bool IsGuest { get; private set; }
    public Guid? UserId { get; private set; }
    public string DisplayName { get; private set; } = "";
    public bool IsSignedIn => UserId is not null;
    public bool HasSession => IsGuest || IsSignedIn;

    /// <summary>Recupera a sessão salva. Um id sem usuário correspondente é descartado.</summary>
    public async Task<bool> RestoreAsync()
    {
        Reset();
        var saved = await store.ReadAsync();
        if (saved == GuestMarker) IsGuest = true;
        else if (Guid.TryParse(saved, out var id) && await users.GetByIdAsync(id) is { } user) SetUser(user);
        else if (saved is not null) await store.ClearAsync();
        return HasSession;
    }

    public async Task StartGuestAsync()
    {
        await store.SaveAsync(GuestMarker);
        Reset();
        IsGuest = true;
    }

    public async Task SignInAsync(User user)
    {
        await store.SaveAsync(user.Id.ToString());
        Reset();
        SetUser(user);
    }

    public async Task SignOutAsync()
    {
        await store.ClearAsync();
        Reset();
    }

    private void SetUser(User user)
    {
        UserId = user.Id;
        DisplayName = user.DisplayName;
    }

    private void Reset()
    {
        IsGuest = false;
        UserId = null;
        DisplayName = "";
    }
}
