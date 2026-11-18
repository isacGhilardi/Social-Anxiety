# Limpeza de branches — 2026-11-11

Critério: branch excluída quando todos os seus commits já estão em `origin/Main` (`git merge-base --is-ancestor <branch> origin/Main`).

## Mantidas

| Branch | Motivo |
|---|---|
| `Main` | Branch padrão do repositório. |
| `origin/fix/ajustes-conteudo-apoio` | 1 commit (`2136dee`) ainda não integrado à `Main`: ajusta orientações de cuidado nas práticas de apoio. Abrir PR ou descartar com o autor. |
| `docs/limpeza-branches` | Branch deste registro. |

## Excluídas (já integradas à `Main`)

### Locais e remotas

| Branch | Último commit |
|---|---|
| `docs/manual-auth-checkin` | 2026-10-23 |
| `feat/meditacao-guiada` | 2026-10-26 |
| `feature/check-in/dados` | 2026-10-23 |
| `feature/componentes-base-ui` | 2026-10-25 |
| `master` | 2026-09-29 (commit inicial) |

### Somente locais (remota já removida após merge do PR)

| Branch | Último commit |
|---|---|
| `feat/gamificacao-regras` | 2026-11-10 |
| `feature/auth-checkin` | 2026-10-23 |
| `fix/gamificacao-semana5` | 2026-11-10 |

### Somente remotas

| Branch | Último commit |
|---|---|
| `feature/challenge-locked` | 2026-11-09 |
| `feature/historico` | 2026-10-26 |
| `feature/onboarding-ui` | 2026-10-08 |
| `feature/respiracao-rastreio-e2e` | 2026-10-22 |
| `feature/tecnicas` | 2026-10-26 |
| `refactor/onboarding-xaml` | 2026-10-08 |
