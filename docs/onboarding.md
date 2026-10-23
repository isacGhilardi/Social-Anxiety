# Preparação local

## Pré-requisitos

- .NET SDK 10 (`dotnet --version` deve mostrar 10.0.x).
- Workloads MAUI: `dotnet workload list` deve listar `maui-windows` e `android` (e `ios`/`maccatalyst` para Apple). Instalação: `dotnet workload install maui`.
- Windows 10 19041+ para o alvo Windows e para os testes funcionais.
- Opcional: Visual Studio 2026 com a carga de trabalho .NET MAUI; Android SDK/emulador para Android; Mac com Xcode para iOS/MacCatalyst.

## Primeira execução

```powershell
dotnet restore Social-Anxiety.slnx
dotnet build Social-Anxiety/Social-Anxiety.csproj -f net10.0-windows10.0.19041.0
.\Social-Anxiety\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Social-Anxiety.exe
```

Não há variáveis de ambiente nem segredos. Os registros ficam em `checkins.json` dentro da pasta de dados do app.

## Verificar

```powershell
dotnet run --project tests/Calme.Tests
powershell -ExecutionPolicy Bypass -File tests/Testar.Windows.ps1
```

Esperado: `15 passaram; 0 falharam.` e 15 linhas `PASS:` na suíte funcional. O relatório funcional fica em `artifacts/funcionais-windows.json`. A suíte usa identidade e pasta de dados próprias (`Calme Tests/com.calme.functionaltests`) e restaura o arquivo de dados ao final.

## Agentes

Leia [AGENTS.md](../AGENTS.md). [CLAUDE.md](../CLAUDE.md) importa as mesmas instruções. Ferramentas e integrações: [setup.md](setup.md).

## Problemas conhecidos

- `MSB3021`/`MSB3026` "arquivo em uso" no build Windows: o app está aberto (sessão de debug do Visual Studio). Feche o app ou use `tests/Testar.Windows.ps1`, que compila em `artifacts/e2e/`.
- Erros `CS0101`/`CS0111` de tipo duplicado: arquivos antigos voltaram para a raiz de `Social-Anxiety/` num merge. Remova as cópias da raiz; as versões válidas ficam em `App/`, `Core/` e `Data/`.
