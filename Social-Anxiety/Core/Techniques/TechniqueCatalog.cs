namespace Social_Anxiety.Core.Techniques;

public sealed record TechniqueStep(string Marker, string Title, string Description);
public sealed record TechniqueContent(string Key, string Name, string Tag, string Title,
    string Description, IReadOnlyList<TechniqueStep> Steps);

// Textos do Calme_Prototipo.html, conferidos com o Aporte Teórico, seções 2.3 e 4.2.
public static class TechniqueCatalog
{
    public static IReadOnlyList<TechniqueContent> All { get; } = Array.AsReadOnly(new[]
    {
        new TechniqueContent("mindfulness", "Mindfulness", "ATENÇÃO AO PRESENTE · 2 MIN",
            "Um momento de presença.", "Perceba o que está aqui agora. Não é preciso esvaziar a mente.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("1", "Apoie os pés", "Sinta o contato dos pés com o chão e encontre uma posição confortável."),
                new TechniqueStep("2", "Note a respiração", "Observe o ar entrar e sair, sem tentar mudar seu ritmo."),
                new TechniqueStep("3", "Reconheça os pensamentos", "Quando a atenção se afastar, perceba isso sem se criticar."),
                new TechniqueStep("4", "Volte com gentileza", "Retome a atenção ao corpo e ao que você percebe ao redor.")
            })),
        new TechniqueContent("stop", "STOP", "UMA PAUSA EM 4 PASSOS",
            "Pare por um instante.", "A prática STOP pode ajudar você a criar uma pausa antes de agir.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("S", "Pare", "Interrompa o que estiver fazendo, se for seguro. Dê um momento a si."),
                new TechniqueStep("T", "Tome uma respiração", "Respire de forma confortável, sem forçar."),
                new TechniqueStep("O", "Observe", "Perceba pensamentos, emoções, sensações e o que está ao redor."),
                new TechniqueStep("P", "Prossiga com cuidado", "Escolha um próximo passo possível e gentil para você.")
            })),
        new TechniqueContent("acalme", "ACALME-SE", "ACOLHER · OBSERVAR · CONTINUAR",
            "Dê espaço ao que sente.", "Uma versão educativa do ACALME-SE. Faça apenas o que for confortável.",
            Array.AsReadOnly(new[]
            {
                new TechniqueStep("A", "Aceite suas emoções", "Reconheça o que você sente, sem se julgar."),
                new TechniqueStep("C", "Contemple ao redor", "Perceba o ambiente e os detalhes que ajudam você a se orientar no presente."),
                new TechniqueStep("A", "Aja com calma", "Escolha uma ação pequena e possível. Você pode adaptar ou pausar."),
                new TechniqueStep("L", "Libere o ar", "Respire suavemente, sem forçar ou prender o ar se isso incomodar."),
                new TechniqueStep("M", "Mantenha a prática", "Repita com gentileza, se fizer sentido."),
                new TechniqueStep("E", "Examine seus pensamentos", "Perceba as interpretações e considere outras possibilidades."),
                new TechniqueStep("S", "Sorria para o seu esforço", "Reconheça que tentar cuidar de si já é um passo."),
                new TechniqueStep("E", "Espere com abertura", "Observe o próximo momento sem exigir um resultado imediato.")
            }))
    });

    public static TechniqueContent? Find(string? key) => All.FirstOrDefault(t => t.Key == key);
}
