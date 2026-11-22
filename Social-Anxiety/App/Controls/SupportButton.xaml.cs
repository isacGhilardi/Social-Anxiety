namespace Social_Anxiety.Controls;

/// <summary>Botão "Preciso de apoio agora": abre a tela de apoio por cima da página atual.</summary>
public partial class SupportButton : ContentView
{
    public SupportButton()
    {
        InitializeComponent();
    }

    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
