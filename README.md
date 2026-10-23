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

## Documentação

- [Produto e escopo](PRODUCT.md)
- [Preparação local](docs/onboarding.md)
- [Estrutura e decisões técnicas](docs/architecture.md)
- [Design e acessibilidade](docs/design.md)
- [Verificações de qualidade](docs/quality.md)
- [Instruções para agentes](AGENTS.md)
- [Setup e evidências](docs/setup.md)

## Próxima entrega

Definir a identidade do app (`ApplicationId` e `ApplicationTitle`) antes de publicar.
