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
- **Sessão no SecureStorage.** Guardamos só o id do usuário ou a marca de convidado, nunca a senha. Ao abrir: primeira vez, onboarding; com sessão salva, início; sem sessão, tela de acesso. "Sair da minha conta", em Configurações, chama `SessionState.SignOutAsync()`, que apaga a sessão e volta à tela de acesso.
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

### Texto da tela de privacidade

Texto usado na tela `privacy` (`App/Pages/Profile/PrivacyPage.xaml`), conferido com o código em 2026-11-22:

> **O que o Calme guarda**
>
> - Seus check-ins: a nota de 1 a 5, o dia e o horário de cada registro.
> - Seu caminho na trilha de desafios: quais níveis você concluiu, as conquistas e as datas.
> - Se você criar uma conta: seu nome, seu e-mail e um código gerado a partir da senha. A senha em si não é guardada, e o código não permite descobri-la.
> - Qual conta está aberta, ou se você entrou como convidado.
> - Algumas escolhas suas: se você já passou pelas boas-vindas, o tema e as opções da respiração guiada (pular a pausa e reduzir movimentos).
>
> **Onde fica**
>
> Tudo fica só neste aparelho. O Calme não tem servidor e não envia seus dados pela internet. Se o backup do aparelho estiver ligado, o sistema pode incluir os dados do Calme na cópia de segurança.
>
> **Limites**
>
> - Sua conta é uma simulação. Nesta versão, ela serve para experimentar o app, fica só neste aparelho e não pode ser recuperada em outro.
> - Os registros não são criptografados. Quem tiver acesso ao aparelho e aos arquivos dele pode chegar a eles.
> - Quem usa este aparelho vê os mesmos registros. Os check-ins, o progresso e as conquistas não ficam separados por conta: qualquer pessoa que abrir o Calme aqui, com outra conta ou como convidada, verá o que foi registrado.
> - As informações do Calme são para autoconhecimento. Não são resultados de uma avaliação clínica.
>
> **Como apagar**
>
> - Por enquanto, não há como apagar seus dados dentro do app.
> - Sair da conta não apaga nada: só encerra a sessão. Check-ins, progresso, conquistas e a conta continuam no aparelho.
> - Para apagar tudo, limpe os dados do Calme nas configurações do aparelho (no Android, em Apps › Calme › Armazenamento; o caminho muda conforme o modelo) ou desinstale o app.

Base no código, para quem for montar a tela:

- Banco `calme.db3` em `FileSystem.AppDataDirectory`, aberto sem chave de criptografia (`Data/Sqlite/CalmeDatabase.cs`). Tabelas em `CalmeDatabase.Tables`: `users` (`UserRow`: `Id`, `DisplayName`, `Email`, `PasswordHash`, `Salt`, `CreatedAtUtc`), `checkins` (`CheckInRow`: `Id`, `Score`, `RecordedAtUtc`, `LocalDay`), `challenge_progress` (`ChallengeProgressRow`: `Id`, `Level`, `CompletedAtUtc`, só níveis 2 e 3) e `achievements` (`AchievementRow`: `Id`, `Key`, `ObtainedAtUtc`). O nível 1 e a conquista "Primeiro cuidado" são calculados a partir dos check-ins, sem gravação. Nenhuma tabela tem vínculo com usuário.
- `SecureStorage`, chave `calme.session`: o id do usuário ou `guest` (`Data/SecureSessionStore.cs`).
- `Preferences`: `calme.onboarding_done` (`App.xaml.cs`, `OnboardingPage.xaml.cs`), `breathing-skip-hold` e `breathing-reduced-motion` (`BreathePage.xaml.cs`). `calme.theme` (`AppPreferences.cs`), gravado pela tela de configurações. O motivo escolhido nas boas-vindas (`OnboardingState.Motive`) fica só na memória.
- Sem código de rede no projeto. O `AndroidManifest.xml` tem `android:allowBackup="true"` e não pede permissões (as de internet do modelo MAUI foram removidas na v1.0.1).
- Nenhum código apaga check-ins, progresso, conquistas ou contas. `SessionState.SignOutAsync()` só remove `calme.session`; é chamado por "Sair da minha conta" em Configurações.
- No Windows (exe não empacotado), os dados ficam em `%LOCALAPPDATA%\<PublisherDisplayName>\<ApplicationId>\` (`Data\calme.db3`, `Settings\preferences.dat` e `Settings\securestorage.dat`), como na identidade de teste `Calme Tests\com.calme.functionaltests`. Apagar o exe não apaga essa pasta. Desinstalar no iOS pode manter o `calme.session` no Keychain: [a confirmar].

## Documentação

- [Produto e escopo](PRODUCT.md)
- [Preparação local](docs/onboarding.md)
- [Estrutura e decisões técnicas](docs/architecture.md)
- [Design e acessibilidade](docs/design.md)
- [Verificações de qualidade](docs/quality.md)
- [Instruções para agentes](AGENTS.md)
- [Setup e evidências](docs/setup.md)

## Próxima entrega

Passar os gráficos a usar o `ChartAggregator`.
