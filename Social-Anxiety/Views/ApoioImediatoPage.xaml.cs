namespace Social_Anxiety.Views;

public partial class ApoioImediatoPage : ContentPage
{
    public ApoioImediatoPage()
    {
        InitializeComponent();
    }

    private async void OnRespiracaoClicked(
        object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Respiração consciente",
            "Encontre uma posição confortável. Deixe o ar entrar e sair suavemente, sem forçar, respirar mais fundo ou prender o ar. Se sentir tontura, falta de ar ou mais ansiedade, interrompa. Se preferir, observe um objeto ao redor em vez da respiração.",
            "Concluir");
    }

    private async void OnPresenteClicked(
        object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Voltar ao presente",
            "Esta é uma adaptação curta de um exercício de atenção aos sentidos. Se for confortável, observe cinco coisas que consegue ver, quatro sensações de toque e três sons. Use apenas os sentidos disponíveis para você; não precisa completar a contagem. Se o desconforto aumentar, interrompa.",
            "Concluir");
    }
}
