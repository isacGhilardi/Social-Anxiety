namespace Social_Anxiety;

// LISTA DE TESTE do Histórico, só para provar a navegação até o Registro individual.
// Quando a tela de Histórico de verdade ficar pronta, ela só precisa chamar:
//   await Shell.Current.GoToAsync($"record?index={i}");
public class HistoryTestPage : ContentPage
{
    static object Res(string key) => Application.Current!.Resources[key];
    static Color C(string key) => (Color)Res(key);

    public HistoryTestPage()
    {
        Title = "Histórico";
        BackgroundColor = C("PageBg");

        var stack = new VerticalStackLayout { Padding = new Thickness(20), Spacing = 12 };

        stack.Children.Add(new Label
        {
            Text = "Seus registros",
            FontFamily = "Nunito",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = C("TextMain")
        });

        for (int i = 0; i < CheckinRecord.Sample.Count; i++)
        {
            int index = i; 
            var r = CheckinRecord.Sample[i];

            var card = new Border
            {
                Style = (Style)Res("CardList"),
                Content = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        new Label { Text = r.Day, Style = (Style)Res("LblCardTitle") },
                        new Label { Text = $"{r.Score} de 5 · {r.ScoreLabel}", Style = (Style)Res("LblCardBody") }
                    }
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await Shell.Current.GoToAsync($"record?index={index}");
            card.GestureRecognizers.Add(tap);

            stack.Children.Add(card);
        }

        Content = new ScrollView { Content = stack };
    }
}