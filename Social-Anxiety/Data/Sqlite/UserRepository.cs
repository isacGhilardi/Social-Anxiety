using SQLite;
using Social_Anxiety.Core.Auth;

namespace Social_Anxiety.Data;

public sealed class UserRepository(CalmeDatabase database) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var db = await database.GetAsync();
        return (await db.FindAsync<UserRow>(id))?.ToUser();
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail)
    {
        var db = await database.GetAsync();
        return (await db.Table<UserRow>().Where(u => u.Email == normalizedEmail).FirstOrDefaultAsync())?.ToUser();
    }

    public async Task<bool> TryAddAsync(User user)
    {
        var db = await database.GetAsync();
        try
        {
            await db.InsertAsync(UserRow.From(user));
            return true;
        }
        catch (SQLiteException error) when (error.Result == SQLite3.Result.Constraint)
        {
            return false;
        }
    }
}
