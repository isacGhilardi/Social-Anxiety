using Social_Anxiety.Views;
namespace Social_Anxiety
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("record", typeof(RecordPage));
            Routing.RegisterRoute("checkin", typeof(CheckinPage));
            Routing.RegisterRoute("feedback", typeof(FeedbackPage));
            Routing.RegisterRoute("history", typeof(HistoryPage));
            Routing.RegisterRoute("breathe", typeof(BreathePage));
            Routing.RegisterRoute("breatheFinish", typeof(BreatheFinishPage));
            Routing.RegisterRoute("challenge", typeof(ChallengePage));
            Routing.RegisterRoute("challengeActive", typeof(ChallengeActivePage));
            Routing.RegisterRoute("challengeDone", typeof(ChallengeDonePage));
        }
    }
}
        