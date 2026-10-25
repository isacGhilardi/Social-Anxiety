namespace Social_Anxiety;

public class LibraryPage : ContentPage
{
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

    public LibraryPage()
    {
        Title = "Técnicas";
        BackgroundColor = C("PageBg");

        var pill = MakeLabel("PRÁTICAS PARA O SEU DIA", 12, C("CalmeTeal"), bold: true);
        pill.CharacterSpacing = 1;

        var title = MakeLabel("Seu tempo de cuidado.", 28, C("TextMain"), bold: true);
        var intro = MakeLabel("Experimente uma prática por vez. Não é preciso fazer tudo.", 15, C("Muted"));

        var notice = new Border
        {
            Style = (Style)Res("CardMint"),
            Padding = new Thickness(16),
            Content = MakeLabel(
                "Escolha o que funciona para você. Essas práticas não substituem acompanhamento profissional.",
                14, C("CalmeTeal"))
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 12,
                Children =
                {
                    pill,
                    title,
                    intro,
                    MakeItem("🍃", "Mindfulness", "Atenção ao presente · 2 min", "mindfulness"),
                    MakeItem("🎧", "Meditação guiada", "Uma pausa para você · 3 min", "meditation"),
                    MakeItem("💜", "ACALME-SE", "Acolhimento em pequenos passos", "acalme", lilac: true),
                    MakeItem("🌬️", "STOP", "Uma pausa em 4 passos", "stop"),
                    notice
                }
            }
        };
    }

    
    View MakeItem(string icon, string name, string subtitle, string route, bool lilac = false)
    {
        var iconBox = new Border
        {
            Style = (Style)Res(lilac ? "ListIconBoxOnTint" : "ListIconBox"),
            Content = new Label
            {
                Text = icon,
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = name, Style = (Style)Res("LblCardTitle") },
                new Label { Text = subtitle, Style = (Style)Res("LblCardBody") }
            }
        };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 14
        };
        row.Add(iconBox, 0, 0);
        row.Add(texts, 1, 0);

        var card = new Border
        {
            Style = (Style)Res(lilac ? "CardListLilac" : "CardList"),
            Content = row
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await OpenTechnique(name, route);
        card.GestureRecognizers.Add(tap);

        return card;
    }

    
    async Task OpenTechnique(string name, string route)
    {
        await DisplayAlertAsync("Em breve", $"A tela de {name} ainda não foi criada.", "OK");
    }
}