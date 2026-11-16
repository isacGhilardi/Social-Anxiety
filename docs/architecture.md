# Código e arquitetura

## Stack

- .NET SDK 10.0.402 (observado), .NET MAUI 10.0.110 (pacotes NuGet observados no build).
- Projeto único MAUI `Social-Anxiety/Social-Anxiety.csproj`: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`. No Windows, `WindowsPackageType=None` (exe não empacotado).
- XAML compilado por source generator (`MauiXamlInflator=SourceGen`).
- Persistência: SQLite via `sqlite-net-pcl` 1.11.285 (`calme.db3` em `FileSystem.AppDataDirectory`); sessão no `SecureStorage`.
- Testes: `tests/Calme.Core.Tests` (xUnit, `net10.0`) e `tests/Funcionais.Windows.ps1` (UI Automation).

## Mapa do código

```
Social-Anxiety/
  App.xaml(.cs)          App, atalhos OpenSupport/OpenHelp/EnterHomeAsync
  AppShell.xaml(.cs)     Shell, rotas e decisão da tela inicial (boas-vindas, acesso ou início)
  MauiProgram.cs         Fontes, logging de debug e injeção de dependência (serviços e páginas)
  App/Controls/          BottomNavBar
  App/Graphics/          ResumoGrafico (IDrawable do gráfico de 7/30 dias)
  App/Pages/Onboarding/  OnboardingPage (boas-vindas, aviso, motivo, acesso, apoio, ajuda) e AccessPage
  App/Pages/Auth/        SignupPage, LoginPage
  App/Pages/Tracking/    MainPage, CheckinPage, FeedbackPage, HistoryPage, RecordPage
  App/Pages/Breathing/   BreathePage, BreatheFinishPage
  App/Pages/Challenges/  ChallengesPage (trilha e conquistas), ChallengeLockedPage, ChallengeActivePage (detalhe e desafio em andamento), ChallengeDonePage, CompleteChallengeAction
  Views/                 MeditacaoGuiadaPage (rota de empilhar `meditacao-guiada`)
  Core/Auth/             User, SessionState, AuthService, PasswordHasher, SignupValidator, EmailNormalizer, interfaces
  Core/CheckIns/         CheckIn, CheckInService, ICheckInRepository
  Core/Models/           OnboardingState
  Core/Breathing/        BreathingSession (máquina de fases sem dependência de UI)
  Core/Challenges/       ChallengeCatalog, ChallengeService, IChallengeRepository e tipos da trilha e das conquistas
  Data/                  SecureSessionStore (SecureStorage)
  Data/Sqlite/           CalmeDatabase, UserRepository, CheckInRepository e linhas das tabelas
                         ChallengeRepository (challenge_progress e achievements, cartão #16)
  Resources/             Estilos, cores, fontes, imagens
tests/
  Calme.Core.Tests/      xUnit do Core e de Data/Sqlite (arquivos incluídos por link)
  Funcionais.Windows.ps1 Casos de interface
  Testar.Windows.ps1     Build isolado + execução dos casos
  FunctionalBuild.targets Identidade separada (com.calme.functionaltests) para o build de teste
```

A pasta `Views/` aparece como `views/` no disco Windows; o Git registra `Views/`.

## Fronteiras e fluxo

Páginas (XAML + code-behind, sem ViewModels) recebem serviços do `Core` pela injeção de dependência. O `Core` tem modelos, regras, interfaces e serviços, sem referência a MAUI ou SQLite. O `Data` implementa as interfaces com SQLite e `SecureStorage`. Estados compartilhados (`SessionState`, `OnboardingState`) são singletons da injeção de dependência.

## Dados

- `CalmeDatabase` abre `calme.db3` no primeiro acesso, liga o modo WAL e cria as tabelas que faltarem (idempotente).
- `users`: id, nome de exibição, e-mail normalizado (índice único), hash e salt da senha, criação em UTC.
- `checkins`: id, nota 1–5, momento em UTC (ticks) e dia local `aaaa-mm-dd` (índice único). Sem vínculo com usuário.
- `challenge_progress`: id, nível (índice único) e conclusão em UTC. `achievements`: id, chave (índice único) e obtenção em UTC. Só níveis 2 e 3 e suas conquistas.
- Erros de leitura ou gravação viram mensagem na tela; o arquivo não é apagado nem recriado. Sem migrações nem rede.

## Decisões

| Decisão | Motivo | Rever quando |
|---|---|---|
| Build funcional com identidade própria | Não tocar nos dados do app real | Mudar o empacotamento Windows |
| SQLite (`sqlite-net-pcl`) no lugar do JSON, começando vazio, 2026-10-23 | Contas locais e índices únicos (e-mail, dia do check-in); app ainda não publicado, sem dados reais a migrar | Houver sincronização ou backend |
| Remoção dos arquivos antigos da raiz (`CheckinPage.cs`, `CheckinRecord.cs`, `HistoryTestPage.cs`, `MainPage.xaml(.cs)`, `RecordPage.cs`, `ResumoGrafico.cs`), 2026-10-23 | Duplicavam tipos de `App/` e `Core/` e quebravam o build com 20 erros | — |
| Nível 1 da trilha e a conquista "Primeiro cuidado" calculados (existe ao menos um check-in); só os níveis 2 e 3 e suas conquistas são gravados (`challenge_progress`, `achievements`), 2026-11-10 | Não duplicar no banco o que os check-ins já provam; o estado do nível é sempre calculado | O nível 1 passar a exigir mais que um check-in |
| Nível e conquista gravados numa única transação (`TryCompleteLevelWithAchievementAsync`), 2026-11-10 | O app fechar no meio não deixa o nível concluído sem a conquista | — |
| Tela `challenge` (detalhe e aceite) junto da `challengeActive`, na mesma página, 2026-11-10 | Menos navegação; "Aceitar desafio" só troca o estado da página e não grava nada, só "Concluí meu desafio" grava | Aceitar passar a gravar algo (por exemplo, desafio em andamento entre sessões) |
| Telas de desafio do PR #39 em `Views/` removidas, 2026-11-10 | Duplicavam as de `App/Pages/Challenges`, guardavam o progresso em `Preferences` (fora do SQLite e do `Core`) e o PR apagou o `OnLoaded` e as rotas do `AppShell`, quebrando a `Main` (CS0246) | — |
| `FakeTrailSource`, `ITrailSource`, `TrailLevel` e os métodos `TryCompleteLevelAsync` e `TryAddAchievementAsync` do repositório removidos, 2026-11-10 | Sem uso no app; só os testes chamavam os métodos do repositório, e agora semeiam o SQLite direto | — |
| Progresso e conquistas sem vínculo com usuário, como `checkins`, 2026-11-10 | Quem usa o mesmo aparelho vê o mesmo progresso | Houver perfil, várias contas por aparelho ou sincronização |
| `ApoioImediatoPage` (PR #24) e `Views/LibraryPage.cs` removidas, e a meditação guiada passou de `ShellContent` para rota de empilhar (`meditacao-guiada`), 2026-11-16 | A `ApoioImediatoPage` não tinha ponto de acesso (o apoio é a tela "Uma pausa agora" do onboarding) e a `LibraryPage` era uma versão antiga da tela de Técnicas, sem uso (a rota `library` é da `TechniquesPage`). Como `ShellContent`, a meditação virava raiz e o "‹ Voltar" (`GoToAsync("..")`) não tinha para onde voltar | Mover a meditação para `App/Pages/` |
| Textos das telas de desafio alinhados ao `Calme_Prototipo.html` aprovado pela psicóloga, 2026-11-10; sem "Ver minhas conquistas" na tela de parabéns | Fonte única dos textos; ainda não existe tela de perfil | Houver tela de perfil |

## Manutenção

Atualize este mapa quando diretórios, contratos ou fronteiras mudarem.
