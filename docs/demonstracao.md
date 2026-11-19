# Dados de demonstração (apresentação final)

Cartão #44. Serve para mostrar os gráficos semanal e mensal, a tela de Perfil e a trilha de desafios sem esperar semanas de uso real.

## Por que um script

O app não importa dados e a tela de check-in só grava o dia de hoje (um check-in por dia). Por isso os dados entram direto no `calme.db3` com `tests/Carregar-Demonstracao.ps1`, que usa as datas relativas a hoje. Sempre rode no dia do ensaio e no dia da apresentação.

## Conjunto de dados

**Check-ins:** do dia 1 do mês (ou dos últimos 7 dias, se for mais antigo) até ontem. A ansiedade cai de ~4 para ~3, com pequenas oscilações. Hoje fica livre, para fazer um check-in ao vivo. Alguns dias ficam sem registro de propósito:

- Semana: o dia de 5 dias atrás fica vazio, e o gráfico mostra a lacuna sem inventar valor.
- Mês: alguns dias vazios nos blocos antigos, e o gráfico mostra a média de cada bloco.

Exemplo no dia 18/11 (semana, do mais antigo ao mais recente: 4, vazio, 2, 3, 3, 3, hoje vazio; mês: Sem 1 ≈ 4,3, Sem 2 ≈ 3,2, Sem 3 ≈ 3,0, Sem 4 vazia).

**Desafios:** o nível 1 (Perceber) conta sozinho pelos check-ins. O nível 2 ("Um olá, do seu jeito") vem concluído, com a conquista "Pequeno passo". O nível 3 fica disponível para concluir ao vivo. Com `-Completo`, o nível 3 e a conquista "Uma conversa" também entram.

## Como carregar

1. Abra o app uma vez (cria as tabelas) e **feche** por completo.
2. Em um PowerShell, na pasta do projeto: `.\tests\Carregar-Demonstracao.ps1`
3. O script mostra o caminho do banco e faz uma cópia dele em `calme.db3.demo-bak`.
4. Abra o app e entre com a conta de demonstração.

Para voltar ao estado anterior: `.\tests\Carregar-Demonstracao.ps1 -Restaurar` (app fechado).

## Roteiro do ensaio

1. **Início:** mostrar o check-in de hoje (feito ao vivo) e a confirmação.
2. **Histórico, Semana:** ponto de cada dia, lacuna sem registro, legenda da escala.
3. **Histórico, Mês:** os quatro blocos do mês e a tendência.
4. **Detalhe:** tocar em um registro para abrir a tela "Seu registro".
5. **Desafios:** nível 1 e 2 concluídos, nível 3 disponível. Concluir o nível 3 ao vivo.
6. **Meu espaço:** sequência de dias, desafios experimentados e as conquistas.
7. Alternar entre tema claro e escuro no Windows e repetir o Histórico.

## Limites

- O gráfico mensal é sempre o mês de calendário atual. No começo do mês ele tem poucos pontos; o ideal é apresentar a partir do dia 15.
- O script apaga os check-ins e o progresso de desafios do banco. Use só no computador de demonstração.
