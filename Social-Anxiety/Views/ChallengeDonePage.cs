namespace Social_Anxiety.Views;

[QueryProperty(nameof(Novo), "novo")]
public class ChallengeDonePage : ContentPage
{
    private string novo = "False";

    public string Novo
    {
        get => novo;
        set
        {
            novo = value;
            AtualizarMensagem();
        }
    }

    private readonly Label mensagem = new()
    {
        FontSize = 17,
        TextColor = Colors.Gray,
        HorizontalTextAlignment = TextAlignment.Center
    };

    public ChallengeDonePage()
    {
        Title = "Desafio concluído";
        BackgroundColor = Color.FromArgb("#F4F8F6");

        Content = new VerticalStackLayout
        {
            Padding = 28,
            Spacing = 22,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "🏆",
                    FontSize = 65,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text = "Parabéns pelo seu passo!",
                    FontSize = 27,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#286B65"),
                    HorizontalTextAlignment = TextAlignment.Center
                },
                mensagem,
                new Button
                {
                    Text = "Voltar aos desafios",
                    BackgroundColor = Color.FromArgb("#39796D"),
                    TextColor = Colors.White,
                    CornerRadius = 22,
                    HeightRequest = 50,
                    Command = new Command(async () =>
                        await Shell.Current.GoToAsync("//home"))
                }
            }
        };
    }

    private void AtualizarMensagem()
    {
        mensagem.Text = Novo.Equals("True", StringComparison.OrdinalIgnoreCase)
            ? "Seu desafio foi registrado com sucesso! Cada pequeno passo merece ser reconhecido."
            : "Esse desafio já foi registrado anteriormente. Seu progresso foi preservado.";
    }
}