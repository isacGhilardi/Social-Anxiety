# Código e arquitetura

## Stack

- .NET SDK 10.0.402 (observado), .NET MAUI 10.0.110 (pacotes NuGet observados no build).
- Projeto único MAUI `Social-Anxiety/Social-Anxiety.csproj`: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`. No Windows, `WindowsPackageType=None` (exe não empacotado).
- XAML compilado por source generator (`MauiXamlInflator=SourceGen`).
- Testes: `tests/Calme.Tests` (console `net10.0`, sem framework de testes) e `tests/Funcionais.Windows.ps1` (UI Automation).

## Mapa do código

```
Social-Anxiety/
  App.xaml(.cs)          App, acesso ao CheckinStore, atalhos OpenSupport/OpenHelp
  AppShell.xaml(.cs)     Shell e rotas
  MauiProgram.cs         Fontes, logging de debug, DI do OnboardingState
  App/Controls/          BottomNavBar
  App/Graphics/          ResumoGrafico (IDrawable do gráfico de 7/30 dias)
  App/Pages/Onboarding/  OnboardingPage (boas-vindas, aviso, motivo, conta/convidado, apoio, ajuda)
  App/Pages/Tracking/    MainPage, CheckinPage, FeedbackPage, HistoryPage, RecordPage
  App/Pages/Breathing/   BreathePage, BreatheFinishPage
  Views/                 ApoioImediatoPage (PR #24)
  Core/Models/           CheckinRecord, OnboardingState
  Core/Breathing/        BreathingSession (máquina de fases sem dependência de UI)
  Data/                  CheckinStore (JSON local)
  Resources/             Estilos, cores, fontes, imagens
tests/
  Calme.Tests/           Testes de lógica (inclui arquivos de Core/ e Data/ por link)
  Funcionais.Windows.ps1 Casos de interface
  Testar.Windows.ps1     Build isolado + execução dos casos
  FunctionalBuild.targets Identidade separada (com.calme.functionaltests) para o build de teste
```

A pasta `Views/` aparece como `views/` no disco Windows; o Git registra `Views/`.

## Fronteiras e fluxo

Páginas (XAML + code-behind) chamam `App.Checkins` diretamente; não há ViewModels nem camada de serviço. Lógica testável fica em `Core/` e `Data/`, sem dependência de MAUI, e é compilada também por `tests/Calme.Tests`.

## Dados

`CheckinStore` grava `checkins.json` em `FileSystem.Current.AppDataDirectory`, com `lock` em memória. Regras: escala 1–5, um registro por data, `Id` estável ao atualizar o dia. JSON inválido ou fora das regras gera `InvalidDataException` e o arquivo é mantido. Sem banco, migrações ou rede.

## Decisões

| Decisão | Motivo | Rever quando |
|---|---|---|
| JSON local em vez de banco/ORM | Escopo de um usuário por aparelho; dados de saúde não saem do aparelho | Houver contas, sincronização ou consultas além de 30 dias |
| Testes de lógica em console sem xUnit | Já existia e roda sem pacotes extras | A suíte crescer a ponto de precisar de filtros e relatórios |
| Build funcional com identidade própria | Não tocar nos dados do app real | Mudar o empacotamento Windows |
| Remoção dos arquivos antigos da raiz (`CheckinPage.cs`, `CheckinRecord.cs`, `HistoryTestPage.cs`, `MainPage.xaml(.cs)`, `RecordPage.cs`, `ResumoGrafico.cs`), 2026-10-23 | Duplicavam tipos de `App/` e `Core/` e quebravam o build com 20 erros | — |

## Manutenção

Atualize este mapa quando diretórios, contratos ou fronteiras mudarem.
