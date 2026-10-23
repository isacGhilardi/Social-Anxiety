using SQLite;
using Social_Anxiety.Core.Auth;

namespace Social_Anxiety.Data;

[Table("users")]
internal sealed class UserRow
{
    [PrimaryKey] public Guid Id { get; set; }
    [NotNull] public string DisplayName { get; set; } = "";
    [NotNull, Unique(Name = "ux_users_email")] public string Email { get; set; } = "";
    [NotNull] public byte[] PasswordHash { get; set; } = [];
    [NotNull] public byte[] Salt { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }

    public User ToUser() => new(Id, DisplayName, Email, PasswordHash, Salt, DateTime.SpecifyKind(CreatedAtUtc, DateTimeKind.Utc));

    public static UserRow From(User user) => new()
    {
        Id = user.Id, DisplayName = user.DisplayName, Email = user.Email,
        PasswordHash = user.PasswordHash, Salt = user.Salt, CreatedAtUtc = user.CreatedAtUtc
    };
}
