namespace Social_Anxiety.Views;

public partial class ApoioImediatoPage : ContentPage
{
    public ApoioImediatoPage()
    {
        InitializeComponent();
    }

    private async void OnRespiracaoClicked(
        object sender, EventArgs e)
    {
        await DisplayAlert(
            "Respiração consciente",
            "Encontre uma posição confortável. Inspire suavemente e solte o ar devagar, sem forçar. Repita no seu ritmo.",
            "Concluir");
    }

    private async void OnPresenteClicked(
        object sender, EventArgs e)
    {
        await DisplayAlert(
            "Voltar ao presente",
            "Observe cinco coisas que consegue ver, quatro que consegue sentir pelo toque e três sons que consegue ouvir. Faça apenas o que for confortável.",
            "Concluir");
    }
}
