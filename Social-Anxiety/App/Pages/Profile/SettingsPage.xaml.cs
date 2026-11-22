using Social_Anxiety.Core.Auth;

namespace Social_Anxiety;

/// <summary>Tela settings (US23): tema, reduzir movimentos, privacidade, ajuda e sair da conta.</summary>
public partial class SettingsPage : ContentPage
{
    private readonly SessionState session;
    private bool loading;

    public SettingsPage(SessionState session)
    {
        this.session = session;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        loading = true;
        DarkSwitch.IsToggled = Application.Current?.RequestedTheme == AppTheme.Dark;
        ReduceMotionSwitch.IsToggled = AppPreferences.ReduceMotion;
        loading = false;
        // Convidado não tem conta para sair; o protótipo oferece criar uma.
        AccountButton.Text = session.IsSignedIn ? "Sair da minha conta" : "Criar uma conta";
    }

    private void OnDarkToggled(object? sender, ToggledEventArgs e)
    {
        if (!loading) AppPreferences.Theme = e.Value ? ThemePreference.Dark : ThemePreference.Light;
    }

    private void OnReduceMotionToggled(object? sender, ToggledEventArgs e)
    {
        if (!loading) AppPreferences.ReduceMotion = e.Value;
    }

    private async void OnAccountClicked(object? sender, EventArgs e)
    {
        if (!session.IsSignedIn)
        {
            await Shell.Current.GoToAsync("signup");
            return;
        }
        try { await session.SignOutAsync(); }
        catch (Exception)
        {
            // SecureStorage pode falhar no aparelho; a sessão continua e nada é apagado.
            await DisplayAlertAsync("Algo deu errado", "Não foi possível sair agora. Tente de novo.", "OK");
            return;
        }
        await Shell.Current.GoToAsync("//access");
    }

    private async void OnPrivacyClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("privacy");
    private async void OnHelpClicked(object? sender, EventArgs e) => await App.OpenHelp();
    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
