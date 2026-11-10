namespace Social_Anxiety.Views;

public class ChallengePage : ContentPage
{
    public ChallengePage()
    {
        Title = "Desafios";
        BackgroundColor = Color.FromArgb("#F4F8F6");

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 18,
                Children =
                {
                    new Label
                    {
                        Text = "Pequenos passos",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#286B65")
                    },
                    new Label
                    {
                        Text = "Cada pequeno passo é uma oportunidade de aprendizado. Escolha participar no seu próprio ritmo.",
                        FontSize = 16,
                        TextColor = Colors.Gray
                    },
                    new Border
                    {
                        Padding = 20,
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
                                    Text = "Desafio: Cumprimente alguém",
                                    FontSize = 21,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#286B65")
                                },
                                new Label
                                {
                                    Text = "Tente cumprimentar uma pessoa conhecida durante o seu dia, respeitando seu próprio conforto.",
                                    FontSize = 16,
                                    TextColor = Colors.DarkSlateGray
                                },
                                new Label
                                {
                                    Text = "Orientações:\n• Escolha alguém com quem se sinta confortável.\n• Faça uma saudação simples.\n• Não existe obrigação de continuar a conversa.\n• Você pode interromper ou deixar para outro momento.",
                                    FontSize = 15,
                                    TextColor = Colors.DarkSlateGray
                                },
                                new Label
                                {
                                    Text = "A participação é totalmente opcional. Faça no seu tempo.",
                                    FontSize = 14,
                                    FontAttributes = FontAttributes.Italic,
                                    TextColor = Colors.Gray
                                }
                            }
                        }
                    },
                    new Button
                    {
                        Text = "Iniciar desafio",
                        BackgroundColor = Color.FromArgb("#39796D"),
                        TextColor = Colors.White,
                        CornerRadius = 22,
                        HeightRequest = 50,
                        Command = new Command(async () =>
                            await Shell.Current.GoToAsync("challengeActive"))
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
            }
        };
    }
}