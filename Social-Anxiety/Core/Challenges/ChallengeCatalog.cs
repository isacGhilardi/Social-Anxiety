namespace Social_Anxiety.Core.Challenges;

/// <summary>Um passo sugerido dentro de um nível da trilha.</summary>
public sealed record ChallengeStep(int Number, string Title, string Description);

/// <summary>Um nível da trilha de microdesafios. <c>Name</c> é o nome curto da trilha (Perceber, Aproximar, Conversar). O estado (concluído, disponível ou bloqueado) é calculado, nunca guardado.</summary>
public sealed record ChallengeLevel(int Number, string Key, string Name, string Title, string Summary, string Description,
    IReadOnlyList<ChallengeStep> Steps);

// Textos do Calme_Prototipo.html; aprovados pela psicóloga.
public static class ChallengeCatalog
{
    public static IReadOnlyList<ChallengeLevel> All { get; } = Array.AsReadOnly(new[]
    {
        new ChallengeLevel(1, "perceber", "Perceber", "Perceber", "Seu primeiro check-in", "Registre como você está hoje.",
            Array.AsReadOnly(Array.Empty<ChallengeStep>())),
        new ChallengeLevel(2, "aproximar", "Aproximar", "Um olá, do seu jeito.", "Um olá, do seu jeito",
            "Escolha uma pessoa com quem você se sinta seguro(a) e experimente cumprimentá-la.",
            Array.AsReadOnly(new[]
            {
                new ChallengeStep(1, "Escolha uma situação possível", "Pode ser uma pessoa conhecida em um momento tranquilo."),
                new ChallengeStep(2, "Cumprimente do seu jeito", "Um “oi” ou um aceno já pode ser um pequeno passo."),
                new ChallengeStep(3, "Acolha seu esforço", "Observe como foi. Não há uma forma perfeita de fazer isso.")
            })),
        new ChallengeLevel(3, "conversar", "Conversar", "Uma conversa curta.", "Uma conversa curta",
            "Se você quiser, faça uma pergunta simples a alguém com quem se sinta seguro(a).",
            Array.AsReadOnly(new[]
            {
                new ChallengeStep(1, "Escolha uma situação possível", "Pode ser uma pessoa conhecida em um momento tranquilo."),
                new ChallengeStep(2, "Faça uma pergunta simples", "Perguntar como foi o dia já pode ser uma conversa."),
                new ChallengeStep(3, "Acolha seu esforço", "Observe como foi. Não há uma forma perfeita de fazer isso.")
            }))
    });

    /// <summary>O nível pelo número, ou nulo fora de 1 a 3.</summary>
    public static ChallengeLevel? Find(int level) => All.FirstOrDefault(l => l.Number == level);
}
