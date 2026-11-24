# Qualidade

## Comandos

| Comando | O que prova |
|---|---|
| `dotnet test tests/Calme.Core.Tests` | xUnit do `Core` (e de `Data/Sqlite` com banco SQLite real em arquivo temporário): sessão, cadastro, login, hash de senha, check-in, agregação do gráfico e respiração. |
| `powershell -ExecutionPolicy Bypass -File tests/Testar.Windows.ps1` | Compila o alvo Windows em `artifacts/e2e/` com identidade de teste e roda os casos de interface (acesso, cadastro, login, convidado, check-in, histórico, filtros, respiração, apoio, banco inválido). Relatório: `artifacts/funcionais-windows.json`. Filtro de um caso: `tests/Funcionais.Windows.ps1 -AppPath <exe> -Filter <nome>` |
| `dotnet build Social-Anxiety/Social-Anxiety.csproj -f net10.0-android` | Compilação Android, sem emulador |
| `dotnet list Social-Anxiety/Social-Anxiety.csproj package --vulnerable --include-transitive` | Avisos de segurança em pacotes NuGet (consulta `api.nuget.org`) |

Verificação completa antes de PR: testes de lógica + testes funcionais. Inclua o build Android quando mexer em `Platforms/`, `.csproj` ou recursos.

Sem lint configurado. O compilador com `Nullable` é o check de tipos; mantenha zero avisos `CS*` no código do projeto. Adotar `.editorconfig`/analisadores é decisão futura: meça as violações antes e comece com severidade `warning`.

## Regras de base

- Arquivos até ~350 linhas, organizados por responsabilidade. Exceção conhecida: `Resources/Styles/Styles.xaml` (cerca de 500 linhas, estilos do template MAUI e botões Calme).
- Lógica nova fica no `Core/` (sem MAUI nem SQLite) e é testada em `tests/Calme.Core.Tests`, que inclui `Core/**` e `Data/Sqlite/**` por link de código-fonte.
- Leitura e gravação de dados nunca apagam o arquivo existente em caso de erro; a mensagem ao usuário não expõe detalhes internos.
- Interface nova: `AutomationId` em controles e `SemanticProperties` em conteúdo não textual.

## Estratégia de testes

Cubra comportamento, limites e falhas, não a implementação. Bug corrigido ganha teste de regressão (lógica em `Calme.Core.Tests`, fluxo em `Funcionais.Windows.ps1`). Testes usam só dados fictícios. A suíte funcional usa uma identidade própria e grava dados de teste apenas no banco dessa identidade; nunca aponte para o app instalado. Android, iOS e MacCatalyst não têm testes automatizados; validação manual nesses alvos fica registrada no PR.

## Harness dos agentes

Depois de mudar `AGENTS.md`, `CLAUDE.md`, memória ou skills, rode os casos de smoke do Vibe Coding Toolkit (`docs/07-harness-evaluation.md` e prompt 14) em cópias descartáveis fora do repositório. Fonte: `https://github.com/soumatheusgomes/vibe-coding-toolkit`, revisão `790a895` (prompts `docs/prompts/14-harness-evaluation.md` e `docs/prompts/15-harness-improvement.md`). Modo adotado: adaptação nativa do método Harness2. Resultados em `docs/setup.md`.

Melhoria de harness: só com falha observada. Defina tarefa, entradas fixas e checks independentes; compare uma única mudança candidata contra o baseline nas mesmas condições; adote só se melhorar sem afrouxar regras. Invocação: "Leia `docs/prompts/15-harness-improvement.md` do toolkit (revisão registrada acima) e execute a seção Prompt neste projeto."

## Revisão após implementação

Escolha o escopo pela mudança real: comportamento e testes sempre; dados locais e privacidade quando tocar `Data/` ou `Core/Models/`; acessibilidade quando tocar XAML. Mudança não trivial: revisão independente (ex.: `/code-review`, skill `ponytail-review` ou subagente somente leitura) sobre o diff exato e os chamadores afetados. Termine com achados resolvidos ou com decisão registrada e checks passando.

## Revisão de segurança

Prompt reutilizável: `docs/prompts/13-security-review.md` do toolkit, revisão `790a895`, seção "Prompt". Invocação: "Leia o prompt 13 do Vibe Coding Toolkit e execute a seção Prompt neste repositório" (modo base) ou com um intervalo `base..head` (modo diff). Somente leitura; relatório privado fora do repositório (ex.: `%LOCALAPPDATA%\security-reviews\`). `differential-review` não está instalada; o fallback é rastreio manual. Superfície atual: dados de saúde locais em `calme.db3`, contas locais simuladas (hash PBKDF2 da senha no mesmo banco), sessão no `SecureStorage`, sem rede nem segredos versionados.

## Dependências e entrega

Rode a auditoria NuGet ao atualizar pacotes. Atualize o MAUI em grupo (pacotes e workloads), conferindo notas de versão. Checks são comandos explícitos; sem hooks de Git nem bloqueios de push.

## Handoff entre sessões

Trabalho inacabado: registro curto no PR ou na tarefa com objetivo, estado verificado (data, branch, mudanças locais), evidência (comandos e resultados), pendências, bloqueios e próxima ação. Ao retomar, confira o estado real antes de confiar no registro.
