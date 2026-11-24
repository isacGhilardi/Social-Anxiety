# Navegação inferior

Cartão #25. Branch `feat/navegacao-inferior`, a partir da `Main` em `b9e1538`.

| Campo | Conteúdo |
|---|---|
| Nome e objetivo | Barra inferior com as 5 abas (Início, Histórico, Técnicas, Desafios, Perfil), todas tocáveis, com a aba atual em destaque. |
| Responsável e revisão | Isac Ghilardi • Guilherme [a preencher] • 2026-11-16 |
| Estado e versão | Implementado nesta branch. |
| Entrada no fluxo | Rodapé de `MainPage`, `HistoryPage`, `TechniquesPage`, `ChallengesPage` e `ProfilePage`, abaixo de "Preciso de apoio agora". Início → `//home`; Histórico → `//home/history`; Técnicas → `//home/library`; Desafios → `//challenges`; Perfil → `//profile`. Telas de fluxo (check-in, registro, respiração, técnica, desafio, apoio, meditação) não têm a barra. |
| Arquivos e símbolos | `App/Controls/BottomNavBar.xaml` (5 `Button` com `AutomationId` `nav-home`, `nav-history`, `nav-techniques`, `nav-challenges`, `nav-profile`; estilos `NavItem` e `NavItemActive` com `AppThemeBinding`; fundo `CalmeSurface`/`CalmeSurfaceDark` no lugar do `White` fixo); `App/Controls/BottomNavBar.xaml.cs` (`NavTab`, `ActiveProperty`, `UpdateItems`, `OnItemClicked`); as 5 páginas acima (`Active="Home"`, `"History"`, `"Techniques"`, `"Challenges"`, `"Profile"`); caso "Barra inferior abre cada aba com o destaque certo" em `tests/Funcionais.Windows.ps1`. |
| Funcionamento | A aba ativa vem da propriedade `Active`, definida no XAML de cada página. A aba ativa fica em negrito, com fundo `CalmeBlue`, e o leitor de tela ouve "<nome>, aba atual". Tocar na aba atual não faz nada. Enquanto uma navegação da barra está em andamento, outros toques são ignorados (trava única para todas as barras). As rotas absolutas recomeçam a pilha: de Histórico para Técnicas, "Voltar" leva ao início. |
| Falhas e correção | Antes, só "Desafios" era botão. Os outros itens eram rótulos sem ação, e o destaque ficava fixo em Início. |
| Validação | Windows 11 x64, 2026-11-16. `dotnet build ... -f net10.0-windows10.0.19041.0` e `-f net10.0-android`: 0 avisos, 0 erros. `dotnet test tests/Calme.Core.Tests`: 106/106. `tests/Testar.Windows.ps1`: 22/23 na 1ª execução ("Minimizar pausa a respiração sem retomar automaticamente" falhou com `InvalidOperationException` ao minimizar pela UI Automation, fora da barra) e 23/23 na repetição. Android, iOS e MacCatalyst: sem execução manual. |
| Limitações e próximos passos | Atualizado em 24/11: o visual e a transição da barra mudaram, e os "Voltar ao início" das abas saíram (ver `entrega-ajustes-ux.md`). Antes disso, o botão "Meu espaço" e os "Voltar ao início" das páginas duplicavam a barra. O teste não cobre os toques durante a navegação. Os avisos da meditação (CS0618, CS8622) foram corrigidos aqui com a mesma alteração da `chore/limpeza-telas`. |
