# Design e acessibilidade

## Direção

Tom acolhedor e calmo, sem linguagem clínica. Textos curtos em pt-BR, convite em vez de ordem ("Que tal um pequeno cuidado agora?"). Suporte a tema claro e escuro via `AppThemeBinding`.

## Sistema existente

Fonte da verdade em `Social-Anxiety/Resources/Styles/`:

- `Colors.xaml`: paleta `Calme*` com pares claro/escuro (`CalmeBg`/`CalmeBgDark`, `CalmePrimary`, `CalmeText`, `CalmeMuted`, `CalmeBlue`, `CalmeMint`, `CalmeLilac`, `CalmeTeal` etc.).
- `FlowStyles.xaml`: `FlowPage`, `FlowHeading`, `FlowCopy`, `FlowCard`, `FlowSupportButton`, `FlowScore`.
- `Cards.xaml`: `CardBlue`, `CardMint`, `CardLilac`, variações de atalho e lista.
- `Styles.xaml`: estilos base, incluindo `CalmeButton` e `CalmeTextButton`.
- Fontes Nunito (400/700/800) registradas em `MauiProgram.cs`.

Use as chaves existentes em vez de cores fixas. `Views/MeditacaoGuiadaPage` ainda usa cores literais (`#568D83`, `#E0F0EA`, `#C2E2D8`, `#39796D`, `#286B65`, `#DDE7E2`); migre para tokens ao alterá-la.

## Interação e acessibilidade

- Todo controle interativo tem `AutomationId` (os testes funcionais dependem dele) e rótulo legível.
- Gráficos expõem os valores em `SemanticProperties.Description`.
- Erros aparecem na própria tela, em texto, sem perder dados.
- Respiração guiada pode ser pausada, retomada e cancelada; minimizar o app pausa a sessão.
- A respiração tem opção própria de movimento reduzido (`breathe-reduced-motion` em `BreathePage.xaml`).
- Pendente: revisar o contraste dos pares de cor e verificar se a preferência de movimento reduzido do sistema é respeitada.

## Layout

Telas de coluna única com `ScrollView`, pensadas para celular. No Windows, a janela usa o mesmo layout. Estados a cobrir: vazio (sem registros), erro de leitura e carregamento rápido (síncrono hoje).

## Manutenção

Mudança visual altera os tokens/estilos e esta página quando uma regra mudar.
