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
  Views/                 ApoioImediatoPage (PR #24, sem ponto de acesso)
  Core/Auth/             User, SessionState, AuthService, PasswordHasher, SignupValidator, EmailNormalizer, interfaces
  Core/CheckIns/         CheckIn, CheckInService, ICheckInRepository
  Core/Models/           OnboardingState
  Core/Breathing/        BreathingSession (máquina de fases sem dependência de UI)
  Data/                  SecureSessionStore (SecureStorage)
  Data/Sqlite/           CalmeDatabase, UserRepository, CheckInRepository e linhas das tabelas
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
- Erros de leitura ou gravação viram mensagem na tela; o arquivo não é apagado nem recriado. Sem migrações nem rede.

## Decisões

| Decisão | Motivo | Rever quando |
|---|---|---|
| Build funcional com identidade própria | Não tocar nos dados do app real | Mudar o empacotamento Windows |
| SQLite (`sqlite-net-pcl`) no lugar do JSON, começando vazio, 2026-10-23 | Contas locais e índices únicos (e-mail, dia do check-in); app ainda não publicado, sem dados reais a migrar | Houver sincronização ou backend |
| Remoção dos arquivos antigos da raiz (`CheckinPage.cs`, `CheckinRecord.cs`, `HistoryTestPage.cs`, `MainPage.xaml(.cs)`, `RecordPage.cs`, `ResumoGrafico.cs`), 2026-10-23 | Duplicavam tipos de `App/` e `Core/` e quebravam o build com 20 erros | — |

## Manutenção

Atualize este mapa quando diretórios, contratos ou fronteiras mudarem.
