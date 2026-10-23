using Social_Anxiety.Core.Auth;

namespace Social_Anxiety.Data;

public sealed class UserRepository(CalmeDatabase database) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var db = await database.GetAsync();
        return (await db.FindAsync<UserRow>(id))?.ToUser();
    }
}
