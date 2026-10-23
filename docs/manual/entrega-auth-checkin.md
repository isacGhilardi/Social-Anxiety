# Entrega: autenticação local, check-in e histórico

As linhas citadas referem-se ao commit `6db05b4`.

| Campo | Conteúdo |
|---|---|
| Branch | `feature/auth-checkin` |
| Merge Request | PR #28 (incorporado à `Main` em `5489e23`) |
| Intervalo documentado | `b70a986..6db05b4` (do ponto em que a branch saiu até o último commit dela) |
| Autor dos commits (Git) | isacGhilardi (todos os commits trazem também `Co-Authored-By: Claude Opus 5.5`) |
| Revisor | [a preencher] |
| Data do último commit | 2026-10-23 |
| Estado | Implementado. Nenhuma funcionalidade foi marcada como validada, porque não há teste manual registrado |

## Resumo

- **Banco local:** o app passa a guardar dados num banco SQLite local (`calme.db3`), no lugar do arquivo `checkins.json`. O app começa vazio, sem migrar o JSON.
- **Conta local simulada (sem backend):**
  - cadastro e login, com senha guardada como hash PBKDF2 (SHA-256, 100.000 iterações, salt aleatório de 16 bytes por usuário);
  - e-mail normalizado e único;
  - mensagem única quando o login falha.
- **Sessão:**
  - a sessão fica no `SecureStorage`, guardando só o id do usuário ou a marca de convidado, nunca a senha;
  - o onboarding aparece só na primeira abertura;
  - ao reabrir, o app vai direto ao início (com sessão) ou à tela de acesso (sem sessão).
- **Check-in:**
  - nota de 1 a 5, com o momento em UTC e o dia local;
  - um check-in por dia (salvar de novo substitui);
  - rótulo calculado, não guardado;
  - sem vínculo com usuário.
- **Agregação do gráfico:**
  - semana: os últimos 7 dias locais;
  - mês: o mês de calendário em 4 blocos;
  - dia ou bloco sem registro fica sem valor, nunca zero;
  - há indicação de período vazio.
- **Correção:** a `OnboardingPage` agora usa o `OnboardingState` da injeção de dependência.
- **Testes:**
  - novo projeto xUnit `tests/Calme.Core.Tests` (77 testes), que substitui o projeto console `tests/Calme.Tests`;
  - suíte funcional do Windows ampliada para 19 casos.

## Commits

| Commit | Data | Mensagem |
|---|---|---|
| `804ccf5` | 2026-10-23 | docs: atualiza próxima entrega e registra decisão do apoio |
| `792c166` | 2026-10-23 | test: cria projeto xUnit do Core e adiciona sqlite-net-pcl |
| `b3e6483` | 2026-10-23 | feat(data): adiciona banco SQLite local com inicialização idempotente |
| `af6af3d` | 2026-10-23 | feat(auth): adiciona sessão persistente, tela de acesso e convidado |
| `e62fa14` | 2026-10-23 | feat(auth): adiciona cadastro e login com conta local simulada |
| `19ca9c1` | 2026-10-23 | feat(checkin): persiste check-ins no SQLite com um registro por dia |
| `f1046ac` | 2026-10-23 | feat(checkin): agrega check-ins por semana e mês para o gráfico |
| `6db05b4` | 2026-10-23 | docs: registra decisões técnicas e define identidade do app |

## Funcionalidades documentadas

| # | Funcionalidade | Arquivo | Último commit | Estado |
|---|---|---|---|---|
| 1 | Infraestrutura do banco local (SQLite) | [banco-local-sqlite.md](banco-local-sqlite.md) | `19ca9c1` | Implementado |
| 2 | Sessão, tela de acesso e modo convidado | [sessao-acesso-convidado.md](sessao-acesso-convidado.md) | `e62fa14` | Implementado |
| 3 | Cadastro e login (conta local simulada) | [cadastro-login.md](cadastro-login.md) | `e62fa14` | Implementado |
| 4 | Registro de check-in | [registro-checkin.md](registro-checkin.md) | `19ca9c1` | Implementado |
| 5 | Histórico e gráfico | [historico-grafico.md](historico-grafico.md) | `f1046ac` | Implementado |
| 6 | Correção do OnboardingState | [correcao-onboardingstate.md](correcao-onboardingstate.md) | `af6af3d` | Implementado |

O commit `6db05b4` também mudou a identidade do app, fora das 6 funcionalidades: `ApplicationId` passou para `com.calme.app` e `ApplicationTitle` para `Calme` (`Social-Anxiety/Social-Anxiety.csproj`).

## Arquivos de interface alterados (revisão do Dev UI)

| Arquivo | Situação | O que mudou na interface |
|---|---|---|
| `Social-Anxiety/App/Pages/Auth/SignupPage.xaml` | Novo | Tela "Criar conta": campos de nome, e-mail e senha, erro, botões "Criar minha conta", "Já tenho uma conta", "Continuar como convidado" e "Preciso de apoio agora" |
| `Social-Anxiety/App/Pages/Auth/SignupPage.xaml.cs` | Novo | Envio do cadastro e navegação |
| `Social-Anxiety/App/Pages/Auth/LoginPage.xaml` | Novo | Tela "Entrar": campos de e-mail e senha, erro, botões "Entrar", "Criar uma conta", "Continuar como convidado" e "Preciso de apoio agora" |
| `Social-Anxiety/App/Pages/Auth/LoginPage.xaml.cs` | Novo | Envio do login e navegação |
| `Social-Anxiety/App/Pages/Onboarding/AccessPage.cs` | Novo | Tela "Seu espaço" como raiz do app, sem visual novo (reusa a `OnboardingPage`) |
| `Social-Anxiety/App/Pages/Onboarding/OnboardingPage.xaml` | Alterado | "Continuar como convidado" passa a abrir a sessão de convidado (`CommandParameter="guest"`, l. 202) |
| `Social-Anxiety/App/Pages/Onboarding/OnboardingPage.xaml.cs` | Alterado | Estado pela injeção de dependência, marca de onboarding, botão "‹" escondido sem tela anterior, cadastro e login pelo Shell |
| `Social-Anxiety/App/Pages/Tracking/MainPage.xaml` | Alterado | Saudação com nome (`x:Name="Greeting"`, `AutomationId="home-greeting"`, l. 8) |
| `Social-Anxiety/App/Pages/Tracking/MainPage.xaml.cs` | Alterado | Saudação e dados do SQLite |
| `Social-Anxiety/App/Pages/Tracking/CheckinPage.xaml.cs` | Alterado | Salvamento pelo serviço; o feedback só abre após o sucesso |
| `Social-Anxiety/App/Pages/Tracking/FeedbackPage.xaml.cs` | Alterado | Leitura pelo serviço |
| `Social-Anxiety/App/Pages/Tracking/HistoryPage.xaml` | Alterado | Modelo da lista passa a ser `CheckIn` (l. 4 e 22), sem mudança visual |
| `Social-Anxiety/App/Pages/Tracking/HistoryPage.xaml.cs` | Alterado | Leitura pelo serviço |
| `Social-Anxiety/App/Pages/Tracking/RecordPage.xaml.cs` | Alterado | Leitura pelo serviço |
| `Social-Anxiety/App/Graphics/ResumoGrafico.cs` | Alterado | Desenha a partir de `CheckIn` (nota e dia local), sem mudança visual |
| `Social-Anxiety/AppShell.xaml` | Alterado | Tela neutra de abertura `start` e raiz `access` (l. 9–14) |
| `Social-Anxiety/AppShell.xaml.cs` | Alterado | Decisão da tela inicial e rotas `signup` e `login` |
| `Social-Anxiety/App.xaml.cs` | Alterado | `EnterHomeAsync` e criação das telas de apoio e ajuda com estado compartilhado |

## Validação desta entrega

Execução em 2026-10-23 10:47, Windows 11 x64, sobre a `Main` em `8a86243`. O código das funcionalidades é o de `6db05b4`; o PR #29 só reformatou a `BottomNavBar` e acrescentou uma linha em branco em `ResumoGrafico.cs`.

| Verificação | Resultado |
|---|---|
| `dotnet build Social-Anxiety/Social-Anxiety.csproj -f net10.0-windows10.0.19041.0` | 0 erros, 0 avisos |
| `dotnet build Social-Anxiety/Social-Anxiety.csproj -f net10.0-android` | 0 erros, 0 avisos |
| `dotnet test tests/Calme.Core.Tests` | 77 testes, 77 aprovados, 0 falhas |
| `tests/Testar.Windows.ps1` (UI Automation) | 19 casos, 19 aprovados |
| iOS / MacCatalyst | Não executado |

### Checklist de teste manual

| # | Caso | Situação |
|---|---|---|
| 1 | Cadastrar, fechar o app por completo, reabrir: continua logado na home | Pendente de confirmação (coberto por teste automático no Windows) |
| 2 | Sair da conta: volta para a access | Não executável: a tela de configurações não existe |
| 3 | Login com senha errada: mensagem única | Pendente de confirmação (coberto por teste automático no Windows) |
| 4 | Cadastro com e-mail já usado: mensagem correspondente | Pendente de confirmação (coberto por teste automático no Windows) |
| 5 | Convidado, fechar e reabrir: vai para a home como convidado | Pendente de confirmação (coberto por teste automático no Windows) |
| 6 | Salvar check-in, fechar e reabrir: o registro continua lá | Pendente de confirmação (coberto por teste automático no Windows) |
| 7 | Salvar duas vezes no mesmo dia: fica um registro, com o valor mais recente | Pendente de confirmação (coberto por teste automático no Windows) |

## Limitações gerais

- **Segurança:** a conta local não é autenticação de segurança real. Quem tem acesso ao aparelho tem acesso ao banco `calme.db3`, que não é criptografado.
- **Dados compartilhados:** quem usa o mesmo aparelho vê os mesmos check-ins; eles não têm vínculo com usuário.
- **Fora do MVP:** backend, recuperação e troca de senha, exclusão de conta, sincronização, telas `settings` e `privacy`, uso do `ChartAggregator` pelas telas.
- **Arquivo `.docx`:** não foi gerado, porque o pandoc não está instalado nesta máquina.
