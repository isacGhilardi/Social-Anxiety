# Limpeza de telas e navegação da meditação guiada

Cartões #27 e #28. Branch `chore/limpeza-telas`, a partir da `Main` em `600cc5b`.

| Campo | Conteúdo |
|---|---|
| Nome e objetivo | #27: tirar do projeto as telas sem uso. #28: fazer o "‹ Voltar" da meditação guiada voltar ao início. |
| Responsável e revisão | Isac Ghilardi • Guilherme [a preencher] • 2026-11-16 |
| Estado e versão | #27: `Views/ApoioImediatoPage.xaml(.cs)`, `Views/LibraryPage.cs` e o `ShellContent` `apoio-imediato` já tinham saído no `4a2e303` (PR #43); esta entrega só atualiza a documentação. #28: implementado nesta branch. |
| Entrada no fluxo | Início → "Meditação guiada" (`home-meditacao`) → `GoToAsync("meditacao-guiada")` empilha a página → "‹ Voltar" (`meditacao-voltar`) → `GoToAsync("..")` volta ao início. |
| Arquivos e símbolos | `AppShell.xaml` (sem o `ShellContent` da meditação; `xmlns:views` mantido); `AppShell.xaml.cs` (`Routing.RegisterRoute("meditacao-guiada", typeof(Views.MeditacaoGuiadaPage))`); `MauiProgram.cs` (`AddTransient<Views.MeditacaoGuiadaPage>()`); `Views/MeditacaoGuiadaPage.xaml` (`AutomationId` `meditacao-voltar`, `meditacao-title`, `meditacao-progresso`, `meditacao-tempo`, `meditacao-reproduzir`, `meditacao-recomecar` e `SemanticProperties`); `Views/MeditacaoGuiadaPage.xaml.cs` (`DisplayAlertAsync`, `sender` anulável). |
| Funcionamento | Antes, a meditação era um `ShellContent` (raiz do Shell): o "‹ Voltar" chamava `GoToAsync("..")` sem página anterior. Agora é rota de empilhar, como `breathe` e `library`. Ao sair da página, o cronômetro para (comportamento já existente). |
| Falhas e correção | Avisos CS0618 (`DisplayAlert` obsoleto) e CS8622 (`sender` não anulável) da meditação, citados na entrega da gamificação → corrigidos. |
| Validação | Windows 11 x64, 2026-11-16. `dotnet build ... -f net10.0-windows10.0.19041.0` e `-f net10.0-android`: 0 avisos, 0 erros. `dotnet test tests/Calme.Core.Tests`: 106/106. `tests/Testar.Windows.ps1`: 23/23, com o caso novo "Meditação abre pelo início e volta ao início". Android, iOS e MacCatalyst: sem execução manual. |
| Limitações e próximos passos | A página continua em `Views/` e com cores literais (ver `docs/design.md`). Não tem botão "Preciso de apoio agora" (cartão #26). |
