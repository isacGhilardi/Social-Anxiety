# Calme

App de apoio à ansiedade social: check-in emocional diário, histórico, respiração guiada e apoio imediato. Os dados ficam só no aparelho. Detalhes em [PRODUCT.md](PRODUCT.md).

## Rodar

Pré-requisitos: .NET SDK 10 e workloads MAUI (`dotnet workload install maui`). No Windows, abra `Social-Anxiety.slnx` no Visual Studio e use o perfil `Windows Machine`.

```powershell
dotnet restore Social-Anxiety.slnx
dotnet test tests/Calme.Core.Tests                                       # testes do Core (xUnit)
powershell -ExecutionPolicy Bypass -File tests/Testar.Windows.ps1        # build isolado + testes funcionais
dotnet build Social-Anxiety/Social-Anxiety.csproj -f net10.0-android     # build Android
```

Não há variáveis de ambiente nem segredos. Os dados ficam num banco SQLite local (`calme.db3`), criado na primeira execução.

## Decisões técnicas

- **Conta local simulada.** Não há backend. Cadastro e login existem para simular o fluxo no MVP; a conta fica só no aparelho, no banco SQLite `calme.db3`. Não há recuperação de senha nem sincronização.
- **Senha com PBKDF2.** A senha nunca é guardada. Guardamos o hash `Rfc2898DeriveBytes.Pbkdf2` com SHA-256, 100.000 iterações e salt aleatório de 16 bytes por usuário; a verificação usa `CryptographicOperations.FixedTimeEquals`. O e-mail é normalizado (sem espaços nas pontas, minúsculo) antes de gravar e de buscar, e é único (índice no banco e checagem no serviço). Login com falha sempre mostra "E-mail ou senha incorretos.".
- **Sessão no SecureStorage.** Guardamos só o id do usuário ou a marca de convidado, nunca a senha. Ao abrir: primeira vez, onboarding; com sessão salva, início; sem sessão, tela de acesso. `SessionState.SignOutAsync()` apaga a sessão; ainda não há tela de configurações com o botão "Sair da minha conta".
- **Um check-in por dia.** Salvar de novo no mesmo dia substitui o anterior (mesmo id). A regra fica no `CheckInService` e no índice único do dia local no banco. Notas fora de 1 a 5 são recusadas. O rótulo (Pouca, Leve, Moderada, Intensa, Muito intensa) é calculado, não guardado. Os check-ins ficam no aparelho, sem vínculo com usuário.
- **UTC + dia local.** Cada check-in guarda o momento em UTC e o dia local, calculado no fuso do aparelho no momento do salvamento. Um check-in às 22h em UTC-3 é do mesmo dia local, mesmo que em UTC já seja o dia seguinte.
- **Semana e mês no gráfico.** Semana: os últimos 7 dias locais terminando hoje, um ponto por dia. Mês: o mês de calendário em 4 blocos (dias 1–7, 8–14, 15–21 e 22 até o fim do mês), rotulados "Sem 1" a "Sem 4", com a média das notas do bloco. Registros fora do período são ignorados.
- **Ausência de dado nunca vira zero.** Dia ou bloco sem registro tem valor nulo; o gráfico não deve desenhá-lo como 0.

### Dados do gráfico (para quem faz a interface)

`ChartAggregator` (em `Core/CheckIns`, registrado na injeção de dependência) recebe os check-ins e devolve um `ChartSeries`:

```csharp
var semana = aggregator.Week(await checkIns.GetLastDaysAsync(7));
var mes = aggregator.CurrentMonth(await checkIns.GetAllAsync());   // ou aggregator.Month(lista, 2026, 10)

if (semana.IsEmpty) { /* mostrar o estado vazio */ }
foreach (var ponto in semana.Points)   // do mais antigo ao mais recente
{
    // ponto.Label: "17".."23" na semana; "Sem 1".."Sem 4" no mês
    // ponto.Value: double? — nulo sem registro (não desenhe como zero)
    // ponto.Count: quantidade de check-ins considerados no ponto
}
```

As telas atuais ainda desenham o gráfico com a lista de check-ins (`ResumoGrafico`); o filtro "Mês" do histórico mostra os últimos 30 dias.

### Check-in na interface

`CheckInService` (injeção de dependência): `SaveTodayAsync(nota)` salva ou substitui o de hoje e devolve o registro; `GetTodayAsync()`, `GetLastDaysAsync(dias)`, `GetByIdAsync(id)` e `GetAllAsync()` (do mais recente ao mais antigo). As telas de check-in, feedback, histórico, registro e início já usam o serviço; a tela de feedback só abre depois que o salvamento deu certo.

### Texto sugerido para a tela de privacidade

A tela `privacy` ainda não existe. Texto sugerido, no tom do protótipo:

> **Seus dados ficam aqui com você.** Tudo o que você registra no Calme fica guardado só neste aparelho. Nada é enviado para a internet.
>
> **Sua conta é uma simulação.** Nesta versão, criar uma conta serve para experimentar o fluxo do app. Ela também fica só neste aparelho e não pode ser recuperada em outro.
>
> **Quem usa este aparelho vê os mesmos registros.** Os check-ins não ficam separados por conta. Se outra pessoa usar o Calme neste aparelho, ela verá o que foi registrado aqui.
>
> As informações do Calme são para autoconhecimento. Não são resultados de uma avaliação clínica.

## Documentação

- [Produto e escopo](PRODUCT.md)
- [Preparação local](docs/onboarding.md)
- [Estrutura e decisões técnicas](docs/architecture.md)
- [Design e acessibilidade](docs/design.md)
- [Verificações de qualidade](docs/quality.md)
- [Instruções para agentes](AGENTS.md)
- [Setup e evidências](docs/setup.md)

## Próxima entrega

Criar as telas `settings` (com "Sair da minha conta" ligado a `SessionState.SignOutAsync()`) e `privacy` (com o texto acima), e passar os gráficos a usar o `ChartAggregator`.
