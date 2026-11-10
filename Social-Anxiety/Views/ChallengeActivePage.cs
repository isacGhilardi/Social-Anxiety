namespace Social_Anxiety.Views;

public class ChallengeActivePage : ContentPage
{
    public ChallengeActivePage()
    {
        Title = "Desafio em andamento";
        BackgroundColor = Color.FromArgb("#F4F8F6");

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Seu desafio está em andamento!",
                    FontSize = 27,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#286B65"),
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Border
                {
                    Padding = 22,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                 CornerRadius = 18
                },
                    BackgroundColor = Colors.White,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 14,
                        Children =
                        {
                            new Label
                            {
                                Text = "Cumprimente alguém",
                                FontSize = 22,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#286B65")
                            },
                            new Label
                            {
                                Text = "Quando se sentir confortável, tente cumprimentar uma pessoa conhecida. Uma saudação simples já é suficiente.",
                                FontSize = 16,
                                TextColor = Colors.DarkSlateGray
                            },
                            new Label
                            {
                                Text = "Lembre-se: você pode fazer uma pausa ou desistir. O desafio deve respeitar seu ritmo.",
                                FontSize = 14,
                                TextColor = Colors.Gray
                            }
                        }
                    }
                },
                new Button
                {
                    Text = "Registrar conclusão",
                    BackgroundColor = Color.FromArgb("#39796D"),
                    TextColor = Colors.White,
                    CornerRadius = 22,
                    HeightRequest = 50,
                    Command = new Command(async () =>
                    {
                        bool novaConclusao =
                            ChallengeProgress.RegistrarConclusao();

                        await Shell.Current.GoToAsync(
                            $"challengeDone?novo={novaConclusao}");
                    })
                },
                new Button
                {
                    Text = "Voltar",
                    BackgroundColor = Color.FromArgb("#DDEFEA"),
                    TextColor = Color.FromArgb("#286B65"),
                    CornerRadius = 22,
                    HeightRequest = 45,
                    Command = new Command(async () =>
                        await Shell.Current.GoToAsync(".."))
                }
            }
        };
    }
}