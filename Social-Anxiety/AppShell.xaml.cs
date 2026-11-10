using Social_Anxiety.Core.Auth;

namespace Social_Anxiety
{
    public partial class AppShell : Shell
    {
        private readonly SessionState session;

        public AppShell(SessionState session)
        {
            this.session = session;
            InitializeComponent();
            Loaded += OnLoaded;

            Routing.RegisterRoute("record", typeof(RecordPage));
            Routing.RegisterRoute("checkin", typeof(CheckinPage));
            Routing.RegisterRoute("feedback", typeof(FeedbackPage));
            Routing.RegisterRoute("history", typeof(HistoryPage));
            Routing.RegisterRoute("breathe", typeof(BreathePage));
            Routing.RegisterRoute("breatheFinish", typeof(BreatheFinishPage));
            Routing.RegisterRoute("signup", typeof(SignupPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("library", typeof(TechniquesPage));
            Routing.RegisterRoute("technique", typeof(TechniquePage));
            Routing.RegisterRoute("challengeLocked", typeof(ChallengeLockedPage));
        }

        // Onboarding só na primeira abertura; depois, home com sessão salva ou tela de acesso sem ela.
        private async void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            if (!Preferences.Default.Get(App.OnboardingDoneKey, false))
            {
                await GoToAsync("//welcome", false);
                return;
            }
            bool hasSession;
            try { hasSession = await session.RestoreAsync(); }
            catch (Exception) { hasSession = false; }
            await GoToAsync(hasSession ? "//home" : "//access", false);
        }
    }
}

