namespace Social_Anxiety;

[QueryProperty(nameof(IndexText), "index")]
public class RecordPage : ContentPage
{
    readonly Label lblDay;
    readonly Label lblScore;

    static object Res(string key) => Application.Current!.Resources[key];
    static Color C(string key) => (Color)Res(key);

    static Label MakeLabel(string text, double size, Color color, bool bold = false) => new Label
    {
        Text = text,
        FontFamily = "Nunito",
        FontSize = size,
        TextColor = color,
        FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None
    };

    static Border MakeCard(string styleKey, View content) => new Border
    {
        Style = (Style)Res(styleKey),
        Content = content
    };

    static Label StyledLabel(string text, string styleKey) => new Label
    {
        Text = text,
        Style = (Style)Res(styleKey)
    };

    public string IndexText
    {
        set
        {
            int.TryParse(value, out var i);
            Show(i);
        }
    }

    public RecordPage()
    {
        Title = "Seu registro";
        BackgroundColor = C("PageBg");

        var pill = MakeLabel("UM RETRATO DO SEU DIA", 12, C("Primary"), bold: true);
        pill.CharacterSpacing = 1;

        lblDay = MakeLabel("", 28, C("TextMain"), bold: true);

        lblScore = StyledLabel("", "LblCardBody");
        var scoreCard = MakeCard("CardBlue", new VerticalStackLayout
        {
            Spacing = 6,
            Children = { StyledLabel("Como você se percebeu", "LblCardTitle"), lblScore }
        });

        var notice = MakeCard("CardMint", MakeLabel(
            "Um registro não define você. Observe o que muda ao longo do tempo, sem se cobrar.",
            14, C("CalmeTeal")));
        ((Border)notice).Padding = new Thickness(16);

        var talkCard = MakeCard("CardBlue", new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                StyledLabel("Leve para uma conversa", "LblCardTitle"),
                StyledLabel("Se quiser, use seus registros para contar a um profissional como têm sido seus dias.", "LblCardBody")
            }
        });

        var btnHistory = new Button
        {
            Text = "Ver meu histórico",
            Style = (Style)Res("BtnPrimary")
        };
        btnHistory.Clicked += OnHistoryClicked;

        var btnHelp = new Button
        {
            Text = "Buscar ajuda profissional",
            Style = (Style)Res("BtnSecondary")
        };
        btnHelp.Clicked += OnHelpClicked;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 12,
                Children = { pill, lblDay, scoreCard, notice, talkCard, btnHistory, btnHelp }
            }
        };

        Show(0);
    }

    void Show(int index)
    {
        var list = CheckinRecord.Sample;
        if (index < 0 || index >= list.Count) index = 0;

        var r = list[index];
        lblDay.Text = r.Day;
        lblScore.Text = $"Ansiedade percebida: {r.Score} de 5 · {r.ScoreLabel}";
    }

    async void OnHistoryClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void OnHelpClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("Em breve", "A tela de ajuda profissional ainda não foi criada.", "OK");
    }
}