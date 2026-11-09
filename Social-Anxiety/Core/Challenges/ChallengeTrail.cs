namespace Social_Anxiety.Core.Challenges;

public enum TrailState { Available, Completed, Locked }

// Um nível da trilha de microdesafios (contrato usado nas telas da trilha e do nível bloqueado).
public sealed record TrailLevel(int Level, string Title, string Description, TrailState State);

public interface ITrailSource
{
    Task<IReadOnlyList<TrailLevel>> GetTrailAsync();
}

// Dados falsos enquanto o contrato real de dados não chega.
// Quando chegar, basta trocar por uma fonte que leia o progresso salvo.
public sealed class FakeTrailSource : ITrailSource
{
    public Task<IReadOnlyList<TrailLevel>> GetTrailAsync() => Task.FromResult<IReadOnlyList<TrailLevel>>(
    [
        new(1, "Um oi de cada vez", "Cumprimente uma pessoa com um sorriso ou um \"oi\".", TrailState.Completed),
        new(2, "Uma conversa curta", "Troque duas ou três frases com alguém, sem pressa.", TrailState.Available),
        new(3, "Um pedido em voz alta", "Peça uma informação simples, como as horas ou um caminho.", TrailState.Locked),
    ]);
}
