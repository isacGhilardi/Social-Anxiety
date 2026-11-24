# Design e acessibilidade

## Direção

Tom acolhedor e calmo, sem linguagem clínica. Textos curtos em pt-BR, convite em vez de ordem ("Que tal um pequeno cuidado agora?"). Suporte a tema claro e escuro via `AppThemeBinding`.

## Sistema existente

Fonte da verdade em `Social-Anxiety/Resources/Styles/`:

- `Colors.xaml`: paleta `Calme*` com pares claro/escuro. Além das cores de base (`CalmeBg`, `CalmeSurface`, `CalmeText`, `CalmeMuted`, `CalmeBlue`, `CalmeMint`, `CalmeLilac`, `CalmeTeal`...), há os tokens de 24/11: `CalmeAction`/`CalmeOnAction` (fundo e texto de botão cheio), `CalmeTonal`/`CalmeTonalBorder`/`CalmeOnTonal` (botão secundário), `CalmeOutline` (borda de cartão), `CalmeSupportBorder`, `CalmeBar`, `CalmeIndicator`, `CalmeNavActive`, `CalmeNavInactive` e `CalmePressed`.
- `Styles.xaml`: botões e estilos implícitos. Button, Entry, Editor e Switch usam Nunito e os tokens Calme; o estado Desabilitado do Button ainda usa os cinzas do template. Os campos de texto ficam dentro de um `Border` (`FlowCard`), e o `MauiProgram` tira a borda nativa do Entry para não haver caixa dupla.
- `FlowStyles.xaml`: `FlowPage`, `FlowHeading`, `FlowCopy`, `FlowSmall`, `FlowEyebrow`, `FlowCard` (com borda de 1 px), `FlowTile`, `FlowListCard`, `FlowIconBox*`, `FlowIcon*`, `FlowSupportButton`, `FlowScore`.
- `Cards.xaml`: obsoleto, fora do `App.xaml`. Pode ser apagado.
- Fontes Nunito (400/700/800) registradas em `MauiProgram.cs` como `Nunito`, `NunitoBold` e `NunitoExtraBold`.

Use as chaves existentes em vez de cores fixas, sempre com `AppThemeBinding`. Fundo de botão é `CalmeAction`. `CalmePrimary`/`CalmePrimaryDark` é só para texto, ícone e linha de gráfico: no escuro ele é claro e ofusca como fundo.

### Hierarquia de botões

| Estilo | Uso |
|---|---|
| `CalmeButton` | A ação principal da tela, no máximo uma (sem contar o apoio). |
| `CalmeSecondaryButton` | Ações comuns e navegação, com box: fundo suave e borda fina. Inclui "Buscar ajuda profissional" e "Sair da minha conta". |
| `CalmeLinkButton` | Ações terciárias ("Voltar", "Ler a política", "Já tenho uma conta"). |
| `FlowSupportButton` | Só "Preciso de apoio agora". |
| `CalmeOverlayButton` | Camada de toque invisível sobre um cartão (`FlowTile`, `FlowListCard`). O conteúdo fica com `InputTransparent="True"`. |
| `CalmeTextButton` | Legado. Continua só na `OnboardingPage`: camada de toque dos cartões e o botão "‹". |

Cartões tocáveis continuam sendo `Button` por cima do conteúdo, porque os testes funcionais acionam os controles por `InvokePattern`.

### Grade e ícones

- Página: `Padding="24,8,24,24"`, `Spacing="16"`. Dentro de cartão: `Spacing="12"`.
- Atalhos em grade de 2 colunas (`ColumnSpacing`/`RowSpacing` 12) com `FlowTile` de mesmo tamanho. Listas de opções com `FlowListCard` (ícone, título, subtítulo e seta).
- Ícones são `Path` de 24×24 (`Aspect="None"`) com os estilos `FlowIcon*`. No C#, use o alias `ShapePath` para não conflitar com `System.IO.Path`.

## Interação e acessibilidade

- Todo controle interativo tem `AutomationId` (os testes funcionais dependem dele) e rótulo legível.
- Gráficos expõem os valores em `SemanticProperties.Description`.
- Erros aparecem na própria tela, em texto, sem perder dados.
- Respiração guiada pode ser pausada, retomada e cancelada; minimizar o app pausa a sessão.
- A respiração tem opção própria de movimento reduzido (`breathe-reduced-motion` em `BreathePage.xaml`).
- Contraste revisado em 24/11: todos os pares de texto dos tokens passam de 4,5:1 nos dois temas. O botão de apoio ganhou borda (`CalmeSupportBorder`) para se destacar do fundo.
- Barra inferior: o indicador da aba ativa tem pouco contraste com a barra (1,5:1 no claro, 2,1:1 no escuro). Isso foi uma escolha: a aba ativa também se distingue pelo ícone e rótulo mais escuros, pelo rótulo em negrito e pelo nome "<aba>, aba atual" no leitor de tela.
- A barra inferior respeita "Reduzir movimentos" (`AppPreferences.ReduceMotion`): com a opção ligada, o indicador muda de aba sem deslizar.
- Pendente: verificar se a preferência de movimento reduzido do sistema operacional também é respeitada.

## Layout

Telas de coluna única com `ScrollView`, pensadas para celular. No Windows, a janela usa o mesmo layout. Estados a cobrir: vazio (sem registros), erro de leitura e carregamento rápido (síncrono hoje).

## Manutenção

Mudança visual altera os tokens/estilos e esta página quando uma regra mudar.
