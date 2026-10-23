using Social_Anxiety.Core.Auth;
using Social_Anxiety.Data;

namespace Calme.Core.Tests;

public class SignupValidatorTests
{
    [Theory]
    [InlineData("Ana", true)]
    [InlineData("A", true)]
    [InlineData("12345678901234567890123456789012", true)]
    [InlineData("123456789012345678901234567890123", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(" Ana", false)]
    [InlineData("Ana ", false)]
    [InlineData(null, false)]
    public void Nome_tem_de_1_a_32_caracteres_sem_espacos_nas_pontas(string? name, bool valid) =>
        Assert.Equal(valid, SignupValidator.IsValidName(name));

    [Theory]
    [InlineData("ana@exemplo.com", true)]
    [InlineData("ana.souza+calme@exemplo.com.br", true)]
    [InlineData("ana@exemplo", false)]
    [InlineData("anaexemplo.com", false)]
    [InlineData("ana @exemplo.com", false)]
    [InlineData("ana@@exemplo.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Email_precisa_de_formato_valido(string? email, bool valid) =>
        Assert.Equal(valid, SignupValidator.IsValidEmail(email));

    [Theory]
    [InlineData("123456", true)]
    [InlineData("12345", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Senha_tem_no_minimo_6_caracteres(string? password, bool valid) =>
        Assert.Equal(valid, SignupValidator.IsValidPassword(password));
}

public class EmailNormalizerTests
{
    [Theory]
    [InlineData("  Ana@Exemplo.COM ", "ana@exemplo.com")]
    [InlineData("ana@exemplo.com", "ana@exemplo.com")]
    [InlineData(null, "")]
    public void Remove_espacos_das_pontas_e_passa_para_minusculas(string? input, string expected) =>
        Assert.Equal(expected, EmailNormalizer.Normalize(input));
}

public class PasswordHasherTests
{
    [Fact]
    public void Senha_correta_e_aceita_e_errada_e_recusada()
    {
        var (hash, salt) = PasswordHasher.Hash("senha-ficticia");
        Assert.True(PasswordHasher.Verify("senha-ficticia", hash, salt));
        Assert.False(PasswordHasher.Verify("senha-errada", hash, salt));
        Assert.False(PasswordHasher.Verify("Senha-ficticia", hash, salt));
    }

    [Fact]
    public void Mesma_senha_com_salts_diferentes_gera_hashes_diferentes()
    {
        var first = PasswordHasher.Hash("senha-ficticia");
        var second = PasswordHasher.Hash("senha-ficticia");
        Assert.Equal(16, first.Salt.Length);
        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
    }

    [Fact]
    public void Hash_nao_contem_a_senha()
    {
        var (hash, _) = PasswordHasher.Hash("senha-ficticia");
        Assert.NotEqual(System.Text.Encoding.UTF8.GetBytes("senha-ficticia"), hash);
        Assert.Equal(32, hash.Length);
    }
}

public class AuthServiceTests
{
    private readonly MemoryUsers users = new();
    private readonly MemorySessionStore store = new();
    private readonly SessionState session;
    private readonly AuthService auth;

    public AuthServiceTests()
    {
        session = new SessionState(store, users);
        auth = new AuthService(users, session, new TestClock(new DateTimeOffset(2026, 10, 23, 15, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task Cadastro_valido_grava_email_normalizado_e_abre_a_sessao()
    {
        var result = await auth.SignUpAsync("  Ana ", " Ana@Exemplo.com ", "senha-ficticia");
        Assert.True(result.Succeeded);
        var user = Assert.Single(users.Users);
        Assert.Equal("Ana", user.DisplayName);
        Assert.Equal("ana@exemplo.com", user.Email);
        Assert.Equal(new DateTime(2026, 10, 23, 15, 0, 0, DateTimeKind.Utc), user.CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, user.CreatedAtUtc.Kind);
        Assert.Equal("Ana", session.DisplayName);
        Assert.Equal(user.Id.ToString(), store.Value);
    }

    [Theory]
    [InlineData("", "ana@exemplo.com", "senha-ficticia")]
    [InlineData("Ana", "ana-exemplo.com", "senha-ficticia")]
    [InlineData("Ana", "ana@exemplo.com", "12345")]
    public async Task Cadastro_invalido_usa_a_mensagem_do_prototipo(string name, string email, string password)
    {
        var result = await auth.SignUpAsync(name, email, password);
        Assert.False(result.Succeeded);
        Assert.Equal("Confira seu nome, e-mail e senha. A senha precisa ter pelo menos 6 caracteres.", result.Error);
        Assert.Empty(users.Users);
        Assert.False(session.HasSession);
    }

    [Fact]
    public async Task Email_ja_cadastrado_mesmo_com_maiusculas_e_recusado()
    {
        await auth.SignUpAsync("Ana", "ana@exemplo.com", "senha-ficticia");
        var result = await auth.SignUpAsync("Outra", "ANA@exemplo.com ", "outra-senha");
        Assert.False(result.Succeeded);
        Assert.Equal("Este e-mail já tem uma conta. Que tal entrar?", result.Error);
        Assert.Single(users.Users);
    }

    [Fact]
    public async Task Login_aceita_email_com_outra_grafia_e_abre_a_sessao()
    {
        await auth.SignUpAsync("Ana", "ana@exemplo.com", "senha-ficticia");
        await session.SignOutAsync();
        var result = await auth.SignInAsync(" ANA@exemplo.com", "senha-ficticia");
        Assert.True(result.Succeeded);
        Assert.Equal("Ana", session.DisplayName);
    }

    [Theory]
    [InlineData("ana@exemplo.com", "senha-errada")]
    [InlineData("nao-existe@exemplo.com", "senha-ficticia")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public async Task Login_com_falha_sempre_usa_a_mesma_mensagem(string? email, string? password)
    {
        await auth.SignUpAsync("Ana", "ana@exemplo.com", "senha-ficticia");
        await session.SignOutAsync();
        var result = await auth.SignInAsync(email, password);
        Assert.False(result.Succeeded);
        Assert.Equal("E-mail ou senha incorretos.", result.Error);
        Assert.False(session.HasSession);
    }
}

public class UserRepositoryTests
{
    private static User NewUser(string email) =>
        new(Guid.NewGuid(), "Pessoa fictícia", email, [1, 2], [3, 4], new DateTime(2026, 10, 23, 15, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task Grava_busca_por_id_e_email_e_preserva_utc()
    {
        await using var temp = new TempDatabase();
        var repository = new UserRepository(temp.Database);
        var user = NewUser("ana@exemplo.com");
        Assert.True(await repository.TryAddAsync(user));

        var byId = await repository.GetByIdAsync(user.Id);
        var byEmail = await repository.GetByEmailAsync("ana@exemplo.com");
        Assert.NotNull(byId);
        Assert.Equal(user.Id, byEmail?.Id);
        Assert.Equal(user.PasswordHash, byId.PasswordHash);
        Assert.Equal(user.CreatedAtUtc, byId.CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, byId.CreatedAtUtc.Kind);
    }

    [Fact]
    public async Task Indice_unico_do_banco_recusa_email_repetido()
    {
        await using var temp = new TempDatabase();
        var repository = new UserRepository(temp.Database);
        Assert.True(await repository.TryAddAsync(NewUser("ana@exemplo.com")));
        Assert.False(await repository.TryAddAsync(NewUser("ana@exemplo.com")));
    }
}
