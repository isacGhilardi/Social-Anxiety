namespace Social_Anxiety.Core.Auth;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
}
