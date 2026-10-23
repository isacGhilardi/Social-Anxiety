using Social_Anxiety;

var passed = 0;
var failed = 0;
void Test(string name, Action test)
{
    try { test(); passed++; Console.WriteLine($"PASS: {name}"); }
    catch (Exception error) { failed++; Console.WriteLine($"FAIL: {name}: {error.Message}"); }
}
void Equal<T>(T actual, T expected)
{
    if (!EqualityComparer<T>.Default.Equals(actual, expected))
        throw new Exception($"Esperado {expected}; recebido {actual}.");
}
void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Esperado {typeof(T).Name}.");
}

var directory = Path.Combine(Path.GetTempPath(), "calme-tests-" + Guid.NewGuid());
Directory.CreateDirectory(directory);
var today = new DateOnly(2026, 10, 22);
try
{
    CheckinStore NewStore(string name) => new(Path.Combine(directory, name + ".json"));
    Test("Arquivo ausente apresenta histórico vazio", () => Equal(NewStore("empty").Read().Count, 0));
    Test("Os cinco valores válidos são persistidos com seu rótulo", () =>
    {
        var store = NewStore("scores");
        foreach (var score in Enumerable.Range(1, 5)) store.Save(score, today.AddDays(-score));
        Equal(new CheckinStore(Path.Combine(directory, "scores.json")).Read().Count, 5);
        Equal(store.Read().Single(r => r.Score == 5).ScoreLabel, "Muito intensa");
        Equal(store.Read().Single(r => r.Score == 1).ScoreLabel, "Pouca");
    });
    Test("Zero e seis são rejeitados sem criar registros", () =>
    {
        var store = NewStore("invalid");
        Throws<ArgumentOutOfRangeException>(() => store.Save(0, today));
        Throws<ArgumentOutOfRangeException>(() => store.Save(6, today));
        Equal(store.Read().Count, 0);
    });
    Test("Salvar novamente no mesmo dia mantém o identificador e atualiza o valor", () =>
    {
        var store = NewStore("update");
        var first = store.Save(4, today);
        var second = store.Save(1, today);
        Equal(first.Id, second.Id);
        Equal(store.Read().Count, 1);
        Equal(store.Read()[0].Score, 1);
    });
    Test("Semana inclui hoje e seis dias anteriores, mas exclui o sétimo", () =>
    {
        var store = NewStore("week");
        foreach (var offset in new[] { 0, 6, 7, -1 }) store.Save(2, today.AddDays(-offset));
        Equal(store.ReadPeriod(7, today).Count, 2);
        Equal(store.ReadPeriod(7, today)[0].Date, today);
    });
    Test("Mês inclui trinta dias e exclui o dia anterior ao intervalo", () =>
    {
        var store = NewStore("month");
        foreach (var offset in new[] { 0, 6, 7, 29, 30 }) store.Save(3, today.AddDays(-offset));
        Equal(store.ReadPeriod(30, today).Count, 4);
    });
    Test("JSON inválido não é apagado ao tentar salvar", () =>
    {
        var path = Path.Combine(directory, "corrupt.json");
        File.WriteAllText(path, "registro quebrado");
        Throws<InvalidDataException>(() => new CheckinStore(path).Save(2, today));
        Equal(File.ReadAllText(path), "registro quebrado");
    });
    Test("Dados locais fora da escala e dias duplicados são rejeitados", () =>
    {
        var path = Path.Combine(directory, "bad-score.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new[] { new CheckinRecord(Guid.NewGuid(), today, 6) }));
        Throws<InvalidDataException>(() => new CheckinStore(path).Read());
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new[] { new CheckinRecord(Guid.NewGuid(), today, 2), new CheckinRecord(Guid.NewGuid(), today, 3) }));
        Throws<InvalidDataException>(() => new CheckinStore(path).Read());
    });
    Test("Falha de gravação mantém o arquivo que impede criar a pasta", () =>
    {
        var path = Path.Combine(directory, "blocked");
        File.WriteAllText(path, "preservar");
        Throws<IOException>(() => new CheckinStore(Path.Combine(path, "data.json")).Save(1, today));
        Equal(File.ReadAllText(path), "preservar");
    });
}
finally { Directory.Delete(directory, true); }
Console.WriteLine($"{passed} passaram; {failed} falharam.");
return failed == 0 ? 0 : 1;

