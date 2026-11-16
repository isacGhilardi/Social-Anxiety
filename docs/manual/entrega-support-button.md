# Controle do botão "Preciso de apoio agora"

Cartão #26. Branch `feat/support-button`, a partir da `Main` em `b9e1538`.

| Campo | Conteúdo |
|---|---|
| Nome e objetivo | Controle reutilizável `SupportButton` com o botão "Preciso de apoio agora", para telas novas não repetirem o botão e o handler. Primeiro uso: meditação guiada, que não tinha o botão. |
| Responsável e revisão | Isac Ghilardi • Guilherme [a preencher] • 2026-11-16 |
| Estado e versão | Implementado nesta branch. |
| Entrada no fluxo | Início → "Meditação guiada" → "Preciso de apoio agora" (rodapé fixo) → "Uma pausa agora" → "Voltar ao que eu estava fazendo" → meditação. |
| Arquivos e símbolos | `App/Controls/SupportButton.xaml` (`ContentView` com um `Button`: texto "Preciso de apoio agora", `Style="{StaticResource FlowSupportButton}"`, `AutomationId="open-support"`, `SemanticProperties.Hint`); `App/Controls/SupportButton.xaml.cs` (`OnSupportClicked` chama `App.OpenSupport()`; namespace `Social_Anxiety.Controls`, como a `BottomNavBar`); `Views/MeditacaoGuiadaPage.xaml` (`Grid RowDefinitions="*,Auto"` com o controle na linha 1, `Padding="24,8,24,12"`, como a `BreathePage`). |
| Funcionamento | Sem parâmetros. O toque empilha a tela de apoio (`OnboardingPage` na rota interna `support`) por cima da página atual, igual ao botão das outras telas. As 16 telas que já tinham o botão (com a `ProfilePage` do PR #44) não mudaram. |
| Validação | Ver o PR: build Windows e Android, `dotnet test tests/Calme.Core.Tests` e `tests/Testar.Windows.ps1`, com o caso novo "Meditação mostra o botão de apoio e o apoio volta à meditação". |
| Limitações e próximos passos | Depende do cartão #28 (`chore/limpeza-telas`): sem ele, tocar em "Meditação guiada" fecha o app (rota relativa para um `ShellContent`), e o caso funcional usa o `meditacao-voltar` criado lá. Trocar o botão das telas antigas pelo controle fica para outro cartão. |
