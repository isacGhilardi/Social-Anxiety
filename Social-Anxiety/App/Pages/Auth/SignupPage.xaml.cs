using Social_Anxiety.Core.Auth;

namespace Social_Anxiety;

public partial class SignupPage : ContentPage
{
    private readonly AuthService auth;
    private readonly SessionState session;
    private bool busy;

    public SignupPage(AuthService auth, SessionState session)
    {
        this.auth = auth;
        this.session = session;
        InitializeComponent();
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        if (busy) return;
        busy = true;
        SubmitButton.IsEnabled = false;
        try
        {
            var result = await auth.SignUpAsync(NameEntry.Text, EmailEntry.Text, PasswordEntry.Text);
            if (result.Succeeded) await App.EnterHomeAsync();
            else ErrorMessage.Text = result.Error;
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        { ErrorMessage.Text = "Não foi possível criar sua conta agora. Tente novamente."; }
        finally { busy = false; SubmitButton.IsEnabled = true; }
    }

    private async void OnLoginClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("../login");
    private async void OnGuestClicked(object? sender, EventArgs e)
    {
        await session.StartGuestAsync();
        await App.EnterHomeAsync();
    }
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
