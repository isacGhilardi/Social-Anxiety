using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

// Cartão #17: ação do botão "Concluí meu desafio".
// A tela só chama: await action.RunAsync(level);
public sealed class CompleteChallengeAction(ChallengeService service)
{
    private bool running;

    public async Task RunAsync(int level)
    {
        if (running) return; // evita dois toques seguidos
        running = true;
        try
        {
            var result = await service.CompleteLevelAsync(level);

            switch (result.Status)
            {
                case CompleteStatus.CompletedNow:
                    var achievement = result.Achievement?.Key ?? "";
                    await Shell.Current.GoToAsync($"challengeDone?level={level}&achievement={achievement}");
                    break;

                case CompleteStatus.AlreadyCompleted:
                    // Já estava concluído: volta para a trilha, sem tela de parabéns.
                    await Shell.Current.GoToAsync("//challenges");
                    break;

                case CompleteStatus.Locked:
                    await Shell.Current.DisplayAlertAsync("Ainda não", "Conclua o nível anterior primeiro.", "OK");
                    break;

                default: // Invalid
                    await Shell.Current.DisplayAlertAsync("Algo deu errado", "Não foi possível concluir este desafio.", "OK");
                    break;
            }
        }
        finally
        {
            running = false;
        }
    }
}
