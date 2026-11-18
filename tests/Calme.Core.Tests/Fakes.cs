using Social_Anxiety.Core.Auth;

namespace Calme.Core.Tests;

sealed class MemorySessionStore : ISessionStore
{
    public string? Value;
    public Task<string?> ReadAsync() => Task.FromResult(Value);
    public Task SaveAsync(string value) { Value = value; return Task.CompletedTask; }
    public Task ClearAsync() { Value = null; return Task.CompletedTask; }
}

sealed class MemoryUsers(params User[] initial) : IUserRepository
{
    public readonly List<User> Users = [.. initial];
    public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));
    public Task<User?> GetByEmailAsync(string email) => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));
    public Task<bool> TryAddAsync(User user)
    {
        if (Users.Any(u => u.Email == user.Email)) return Task.FromResult(false);
        Users.Add(user);
        return Task.FromResult(true);
    }
}
