namespace Social_Anxiety.Core.Auth;

public sealed record AuthResult(bool Succeeded, string? Error)
{
    public const string InvalidSignup = "Confira seu nome, e-mail e senha. A senha precisa ter pelo menos 6 caracteres.";
    public const string EmailTaken = "Este e-mail já tem uma conta. Que tal entrar?";
    public const string InvalidLogin = "E-mail ou senha incorretos.";

    public static readonly AuthResult Success = new(true, null);
    public static AuthResult Fail(string error) => new(false, error);
}
