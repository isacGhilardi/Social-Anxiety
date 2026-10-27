namespace Social_Anxiety.Core.Techniques;

public sealed record TechniqueStep(string Marker, string Title, string Description);
public sealed record TechniqueContent(string Key, string Name, string Tag, string Title,
    string Description, IReadOnlyList<TechniqueStep> Steps);

public static class TechniqueCatalog
{
    public static IReadOnlyList<TechniqueContent> All { get; } = Array.AsReadOnly(new[]
    {
        new TechniqueContent("mindfulness", "Mindfulness", "ATENÇÃO AO PRESENTE · 2 MIN",
            "Um momento de presença.", "Perceba o que está aqui agora. Não é preciso esvaziar a mente nem alcançar um estado de calma.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("1", "Apoie os pés", "Sinta o contato dos pés com o chão e encontre uma posição confortável."),
                new TechniqueStep("2", "Escolha um ponto de atenção", "Se for confortável, observe a respiração sem mudar seu ritmo. Você também pode olhar um objeto ou sentir o apoio dos pés no chão, com os olhos abertos."),
                new TechniqueStep("3", "Reconheça os pensamentos", "Quando a atenção se afastar, perceba isso sem se criticar."),
                new TechniqueStep("4", "Volte com gentileza", "Retome a atenção ao corpo e ao que você percebe ao redor.")
            })),
        new TechniqueContent("stop", "STOP", "UMA PAUSA EM 4 PASSOS",
            "Pare por um instante.", "A prática STOP pode ajudar você a criar uma pausa antes de agir.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("S", "Pare", "Interrompa o que estiver fazendo, se for seguro. Dê um momento a si."),
                new TechniqueStep("T", "Tome uma respiração", "Respire de forma confortável, sem forçar. Se focar na respiração incomodar, observe um objeto ou sinta o apoio dos pés no chão."),
                new TechniqueStep("O", "Observe", "Perceba pensamentos, emoções, sensações e o que está ao redor."),
                new TechniqueStep("P", "Prossiga com cuidado", "Escolha um próximo passo possível e gentil para você.")
            })),
        new TechniqueContent("acalme", "ACALME-SE", "ACOLHER · OBSERVAR · CONTINUAR",
            "Dê espaço ao que sente.", "Adaptação educativa inspirada no ACALME-SE, com palavras e passos ajustados para este app. Não é um protocolo de tratamento validado. Você pode pausar quando precisar.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("A", "Acolha suas emoções", "Reconheça o que você sente, sem se julgar. Acolher uma emoção não significa aceitar uma situação perigosa: nesses casos, priorize sua segurança e procure ajuda."),
                new TechniqueStep("C", "Contemple ao redor", "Perceba o ambiente e os detalhes que ajudam você a se orientar no presente."),
                new TechniqueStep("A", "Aja com calma", "Escolha uma ação pequena e possível. Você pode adaptar ou pausar."),
                new TechniqueStep("L", "Libere o ar", "Deixe a respiração seguir de forma confortável, sem forçar nem precisar prender o ar. Se isso incomodar, volte a atenção para um objeto ao redor."),
                new TechniqueStep("M", "Mantenha a prática", "Repita com gentileza, se fizer sentido."),
                new TechniqueStep("E", "Examine seus pensamentos", "Perceba as interpretações e considere outras possibilidades."),
                new TechniqueStep("S", "Seja gentil com seu esforço", "Reconheça sua tentativa de cuidar de si. Você não precisa sorrir nem se sentir melhor para concluir a prática."),
                new TechniqueStep("E", "Espere com abertura", "Observe o próximo momento sem exigir um resultado imediato.")
            }))
    });

    public static TechniqueContent? Find(string? key) => All.FirstOrDefault(t => t.Key == key);
}
