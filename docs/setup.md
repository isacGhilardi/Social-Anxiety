# Setup e evidências

## Fonte e contexto

- Data: 2026-10-23.
- Toolkit: `https://github.com/soumatheusgomes/vibe-coding-toolkit`, revisão `790a895304e0c08b70a5b215c9d334f1c86b816c` (2026-10-18), clonada em diretório temporário fora do projeto. Prompt aplicado: `docs/prompts/12-existing-project-setup.md`.
- Cliente: Claude Code 2.1.309 (Windows 11, PowerShell/Git Bash). Modelo principal preservado. Autenticação da sessão preservada.
- Política de subagentes: modelo mais leve capaz de cumprir o critério de aceite; tarefas pequenas e verificáveis usaram `sonnet`. Sem API paga, proxy ou troca de provedor.
- Projeto: .NET MAUI 10, SDK 10.0.402, workloads `android 36.1.118`, `ios 26.5`, `maccatalyst 26.5`, `maui-windows 10.0.110`.

## Decisões

- Stack preservada. Partes de Next.js, Drizzle/PostgreSQL, ESLint/Biome, Zod e Pino do toolkit não se aplicam a C#/MAUI; os equivalentes são o compilador com nullable e os testes existentes.
- Nenhuma dependência nova. Nenhum CI criado (o repositório não tem CI; criar um é decisão da equipe).
- Context7 indisponível nesta sessão (pede OAuth); a documentação oficial da Microsoft é a fonte para MAUI.

## Componentes

| Componente | Ação e motivo | Escopo | Estado e evidência |
|---|---|---|---|
| Superpowers | Reusado (plugin 7.0.0) | Usuário | Skill de brainstorming carregada e usada nesta sessão |
| Ponytail (`full`) | Reusado | Usuário | Ativo via hook de sessão |
| Caveman | Reusado | Usuário | Ativo via hook de sessão |
| RTK | Reusado (hook nativo) | Usuário | Saída condensada observada nos comandos; sem wrapper duplicado |
| Context7 | Reusado (plugin MCP) | Usuário | **Bloqueado**: servidor exige OAuth. Próximo passo: rodar `/mcp` no Claude Code e autenticar `context7` |
| Graphify 0.9.92 | Integrado: `.graphifyignore` e grafo só de código | Projeto (saída fora do Git) | `graphify-out/graph.json`: 49 arquivos, 335 nós, 517 arestas, sem cluster nem extração semântica; consulta `graphify explain CheckinStore` funcionou. Atualização manual (sem hook de Git) |
| Vercel skills / find-skills | Não aplicável | — | Projeto não é web nem React Native |
| next-dev-loop, next-bundle-optimizer | Não aplicável | — | Sem Next.js |
| supabase-postgres-best-practices | Não aplicável | — | Sem PostgreSQL |
| gh-fix-ci | Não aplicável | — | Sem GitHub Actions |
| differential-review | Não instalado | — | Fallback: rastreio manual no prompt 13; instalar quando houver PRs que mexam em dados |
| shadcn | Não aplicável | — | UI em XAML |
| Sentry MCP | Não aplicável | — | Sem Sentry nem serviço escolhido |
| agent-browser / Chrome DevTools MCP | Não aplicável | — | App nativo; a automação de UI é feita por `tests/Funcionais.Windows.ps1` (UI Automation) |
| Harness2 (adaptação nativa) | Adotado como procedimento em `docs/quality.md` | Projeto | Smoke inicial abaixo; nenhuma falha observada que justifique comparação de candidato. CLI oficial não instalada |
| Revisão de segurança (prompt 13) | Fonte versionada e invocação em `docs/quality.md` | Projeto | Não executada nesta sessão |
| Memória em camadas | Memória nativa do Claude Code + docs versionados | Usuário/projeto | Índice: `~/.claude/projects/C--Users-Zackzin-source-repos-Social-Anxiety/memory/MEMORY.md` |
| Obsidian, aia-harness, artifact skills, hooks de cliente extras | Não aplicável | — | Sem necessidade concreta |

## Documentação criada

Não existia documentação de projeto. Criados: `README.md`, `PRODUCT.md`, `AGENTS.md`, `CLAUDE.md` (importa `AGENTS.md`), `docs/architecture.md`, `docs/design.md`, `docs/onboarding.md`, `docs/quality.md`, `docs/setup.md`. `PRODUCT.md` foi reconstruído do código e dos testes; itens incertos estão marcados como suposição ou decisão pendente.

## Mudanças no código

- Removidos 7 arquivos antigos da raiz de `Social-Anxiety/` (`CheckinPage.cs`, `CheckinRecord.cs`, `HistoryTestPage.cs`, `MainPage.xaml`, `MainPage.xaml.cs`, `RecordPage.cs`, `ResumoGrafico.cs`). Eram versões anteriores à reorganização em `App/`, `Core/` e `Data/` e voltaram nos merges dos PRs #19–#25. Causavam 20 erros de compilação (tipos duplicados). Recuperáveis pelo histórico do Git.
- `Views/ApoioImediatoPage.xaml.cs`: `DisplayAlert` (obsoleto) trocado por `DisplayAlertAsync` e `sender` anulável nos handlers. Removeu os avisos CS0618 e CS8622.
- `.gitignore`: `graphify-out/`. Novo `.graphifyignore`.
- Apoio (decisão do usuário, seguindo o protótipo `docs/Calme_Prototipo.html`): "Preciso de apoio agora" abre "Uma pausa agora" (Respiração guiada, STOP e ACALME-SE). Nessa tela o botão vira "Voltar ao que eu estava fazendo" e retorna à origem. `Views/ApoioImediatoPage` continua no projeto sem ponto de acesso.

## Checks

| Item | Antes | Depois |
|---|---|---|
| Testes de lógica (`Calme.Tests`) | 15 passaram, 0 falharam | 15 passaram, 0 falharam |
| Build Windows | **Falhava**: 20 erros (CS0101, CS0111, CS0260, CS0579, CS0756, CS0757, CS8863) | 0 erros, 0 avisos (build isolado de `tests/Testar.Windows.ps1`) |
| Testes funcionais Windows | Não rodavam (build quebrado) | 15/15 passaram |
| Build Android | Não medido antes | 0 erros, 0 avisos |
| iOS / MacCatalyst | Não executado | Não executado (exige Mac) |
| Auditoria NuGet | — | Nenhum pacote vulnerável |

O build padrão também mostra `MSB3021`/`MSB3026` enquanto o app roda no Visual Studio (arquivo em uso). Isso não é erro de código.

## Harness

Casos do prompt 14, rodados em 2026-10-23 em cópias descartáveis de `templates/harness-evaluation/` fora do projeto. Avaliador `scripts/validate-harness.mjs` (sha256 `ebc2cdd0…5b8c`) ficou fora do escopo gravável dos agentes. Baseline intacto: 5 checks passam e 2 falham, como esperado. Executores: subagentes `sonnet` com contexto novo (não é reinício do cliente).

| Caso | Resultado | Evidência independente |
|---|---|---|
| Correção de fronteira | Passou | O coordenador rodou o avaliador de novo: 7/7. Só `access.mjs` mudou; `local-notes.txt` e `AGENTS.md` intactos |
| Handoff desatualizado | Passou | O agente rejeitou o handoff que dizia "corrigido, 7/7", reproduziu 5/2, corrigiu e reescreveu o handoff com a evidência real; o avaliador deu 7/7 |
| Revisão de segurança somente leitura | Passou | Hashes da cópia iguais antes e depois. O relatório privado aponta acesso entre tenants com exemplo concreto, local, impacto, severidade e confiança |
| Reaplicação, conflito de recursos, bloqueio parcial, pausa e retomada | Não executados | Exigem interação observável com o host ou um projeto representativo; ficam para uma próxima mudança de harness |

Retomada em contexto limpo: o `CLAUDE.md` deste projeto será carregado na próxima sessão. Isso não foi observado aqui, porque exige reiniciar o cliente.

## Pendências e próxima entrega

- Context7: autenticar via `/mcp`.
- Identidade do app definida em 2026-10-23: `ApplicationId` = `com.calme.app`, `ApplicationTitle` = `Calme`. No Windows, `PublisherDisplayName` do manifesto ainda é `User Name`.
- Teste instável: "Respiração inicia, pausa e conclui" falhou uma vez (`Esperado: Pausa; recebido: Inspire`) e passou na repetição seguinte. Provável corrida de tempo entre o clique e a leitura da fase; investigar se voltar a acontecer.
- Opcional: CI no GitHub Actions com testes de lógica e build Android, se a equipe quiser.
- Repetir este setup: rode o prompt 12 do toolkit com a nova revisão. Ele preserva estes documentos e atualiza esta tabela; sem reinstalar componentes que já funcionam.
