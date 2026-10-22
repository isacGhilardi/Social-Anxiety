namespace Social_Anxiety;

public partial class CheckinPage : ContentPage
{
    int score = 0;
    readonly Button[] choices = new Button[5];
    readonly Label lblSelected;
    readonly Button btnSave;

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

    public CheckinPage()
    {
        Title = "Seu check-in";
        BackgroundColor = C("PageBg");

        // Escala 1 a 5
        var scale = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 8, 0, 0) };
        for (int i = 0; i < 5; i++)
        {
            scale.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var b = new Button { Text = (i + 1).ToString(), CommandParameter = i + 1 };
            b.Clicked += OnScoreClicked;
            Grid.SetColumn(b, i);
            scale.Children.Add(b);
            choices[i] = b;
        }

        // Rótulos "Pouca / Muita ansiedade"
        var scaleLabels = new Grid();
        scaleLabels.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        scaleLabels.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        var left = MakeLabel("1 · Pouca ansiedade", 12, C("Muted"));
        var right = MakeLabel("5 · Muita ansiedade", 12, C("Muted"));
        right.HorizontalTextAlignment = TextAlignment.End;
        Grid.SetColumn(right, 1);
        scaleLabels.Children.Add(left);
        scaleLabels.Children.Add(right);

        lblSelected = MakeLabel("Não há resposta certa ou errada.", 14, C("Muted"));

        // Aviso
        var notice = new Border
        {
            Style = (Style)Res("CardMint"),
            Padding = new Thickness(16),
            Content = MakeLabel(
                "Escolha a percepção que mais se aproxima do seu dia. Você pode mudar antes de salvar.",
                14, C("CalmeTeal"))
        };

        btnSave = new Button
        {
            Text = "Salvar meu check-in",
            Style = (Style)Res("BtnPrimary")
        };
        btnSave.Clicked += OnSaveClicked;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 12,
                Children =
                {
                    MakeLabel("Em que parte do dia?", 24, C("TextMain"), bold: true),
                    MakeLabel("Ao pensar em situações sociais, quanta ansiedade você percebeu?", 14, C("Muted")),
                    scale,
                    scaleLabels,
                    lblSelected,
                    notice,
                    btnSave,
                    MakeLabel("Esta escala é um registro pessoal. Não é um inventário clínico nem um resultado diagnóstico.", 12, C("Muted"))
                }
            }
        };

        UpdateUI();
    }

    void OnScoreClicked(object? sender, EventArgs e)
    {
        score = Convert.ToInt32(((Button)sender!).CommandParameter);
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < choices.Length; i++)
        {
            var b = choices[i];
            bool selected = score == i + 1;

            b.MinimumHeightRequest = 86;
            b.CornerRadius = 16;
            b.Padding = 0;
            b.FontFamily = "Nunito";
            b.FontSize = 16;
            b.FontAttributes = FontAttributes.Bold;

            b.BackgroundColor = selected ? C("SoftBlue") : C("Surface");
            b.BorderColor = selected ? C("Primary") : C("CalmeBorder");
            b.BorderWidth = selected ? 2 : 1;
            b.TextColor = selected ? C("Primary") : C("Muted");
        }

        lblSelected.Text = score == 0
            ? "Não há resposta certa ou errada."
            : $"{score} de 5";

        btnSave.IsEnabled = score > 0;
        btnSave.Opacity = score > 0 ? 1 : 0.62;
    }

    async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (score == 0) return;

        // A gravação definitiva (dados locais) é a tarefa do Dev Backend/Dados.
        await DisplayAlertAsync("Check-in salvo", $"Ansiedade percebida: {score} de 5", "OK");

        score = 0;
        UpdateUI();
    }
}