using Social_Anxiety.Core.Auth;

namespace Social_Anxiety;

public partial class App : Application
{
    public const string OnboardingDoneKey = "calme.onboarding_done";

    private static CheckinStore? checkins;
    public static CheckinStore Checkins => checkins ??= new CheckinStore(Path.Combine(FileSystem.Current.AppDataDirectory, "checkins.json"));
    public static Task OpenSupport() => Shell.Current.Navigation.PushAsync(CreateOnboardingPage("support"), false);
    public static Task OpenHelp() => Shell.Current.Navigation.PushAsync(CreateOnboardingPage("help"), false);

    /// <summary>Depois de abrir uma sessão: o onboarding não aparece mais e a home vira a raiz.</summary>
    public static Task EnterHomeAsync()
    {
        Preferences.Default.Set(OnboardingDoneKey, true);
        return Shell.Current.GoToAsync("//home");
    }

    private static OnboardingPage CreateOnboardingPage(string route)
    {
        var services = IPlatformApplication.Current!.Services;
        return new OnboardingPage(services.GetRequiredService<OnboardingState>(), services.GetRequiredService<SessionState>(), route);
    }

    private readonly SessionState session;

    public App(SessionState session)
    {
        this.session = session;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell(session));
    }
}
