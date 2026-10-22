namespace Social_Anxiety;

public partial class App : Application
{
    private static CheckinStore? checkins;
    public static CheckinStore Checkins => checkins ??= new CheckinStore(Path.Combine(FileSystem.Current.AppDataDirectory, "checkins.json"));
    public static Task OpenSupport() => Shell.Current.Navigation.PushAsync(new OnboardingPage(new OnboardingState(), "support"), false);
    public static Task OpenHelp() => Shell.Current.Navigation.PushAsync(new OnboardingPage(new OnboardingState(), "help"), false);

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
