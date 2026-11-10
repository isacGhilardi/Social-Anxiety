namespace Social_Anxiety.Core.Challenges;

/// <summary>Conclusão gravada de um nível (só 2 e 3; o nível 1 é calculado a partir dos check-ins). Momento em UTC.</summary>
public sealed record ChallengeProgress(Guid Id, int Level, DateTime CompletedAtUtc);

/// <summary>Conquista gravada (só as dos níveis 2 e 3). Momento em UTC.</summary>
public sealed record Achievement(Guid Id, string Key, DateTime ObtainedAtUtc);

/// <summary>Conquista como a tela mostra: obtida ou não, com a data quando gravada. "Em breve" ainda não pode ser obtida.</summary>
public sealed record AchievementInfo(string Key, string Name, bool Obtained, DateTime? ObtainedAtUtc, bool ComingSoon);

/// <summary>Um nível da trilha com o estado calculado no momento da leitura.</summary>
public sealed record TrailItem(ChallengeLevel Level, TrailState State);

/// <summary>Estado de um nível, calculado a cada leitura.</summary>
public enum TrailState { Available, Completed, Locked }

public enum CompleteStatus { CompletedNow, AlreadyCompleted, Locked, Invalid }

/// <summary>Resultado de "Concluí meu desafio". <paramref name="Achievement"/> só vem preenchida em <see cref="CompleteStatus.CompletedNow"/>.</summary>
public sealed record CompleteResult(CompleteStatus Status, AchievementInfo? Achievement);
