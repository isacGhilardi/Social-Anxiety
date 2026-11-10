namespace Social_Anxiety.Views;

public static class ChallengeProgress
{
    private const string ChaveConclusao = "challenge_cumprimentar_completed";

    public static bool EstaConcluido =>
        Preferences.Get(ChaveConclusao, false);

    public static bool RegistrarConclusao()
    {
        if (EstaConcluido)
            return false;

        Preferences.Set(ChaveConclusao, true);
        return true;
    }
}