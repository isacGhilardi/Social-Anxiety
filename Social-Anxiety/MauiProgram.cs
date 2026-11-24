using Microsoft.Extensions.Logging;

namespace Social_Anxiety
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Nunito-400.ttf", "Nunito");
                    fonts.AddFont("Nunito-700.ttf", "NunitoBold");
                    fonts.AddFont("Nunito-800.ttf", "NunitoExtraBold");
                });

            // Os campos de texto ficam dentro de um Border com o box do Calme (Login e Cadastro).
            // Tira a borda e o sublinhado nativos para não desenhar uma caixa dupla.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("CalmeSemBordaNativa", (handler, _) =>
            {
#if WINDOWS
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                // O estado de foco do TextBox reaplica a borda por este recurso.
                handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
#elif IOS || MACCATALYST
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<OnboardingState>();
            builder.Services.AddSingleton(_ => new Data.CalmeDatabase(Path.Combine(FileSystem.AppDataDirectory, "calme.db3")));
            builder.Services.AddSingleton<Core.Auth.ISessionStore, Data.SecureSessionStore>();
            builder.Services.AddSingleton<Core.Auth.IUserRepository, Data.UserRepository>();
            builder.Services.AddSingleton<Core.Auth.SessionState>();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<Core.Auth.AuthService>();
            builder.Services.AddSingleton<Core.CheckIns.ICheckInRepository, Data.CheckInRepository>();
            builder.Services.AddSingleton<Core.Challenges.IChallengeRepository, Data.ChallengeRepository>();
            builder.Services.AddSingleton<Core.CheckIns.CheckInService>();
            builder.Services.AddSingleton<Core.CheckIns.ChartAggregator>();
            builder.Services.AddSingleton<Core.Challenges.ChallengeService>();
            builder.Services.AddSingleton<Core.Profile.ProfileStatsService>();
            // Cartão #17: ação do botão "Concluí meu desafio"; a tela challengeActive recebe pelo construtor.
            builder.Services.AddTransient<CompleteChallengeAction>();
            // Páginas registradas para o Shell criá-las pela injeção de dependência.
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<AccessPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CheckinPage>();
            builder.Services.AddTransient<FeedbackPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<RecordPage>();
            builder.Services.AddTransient<TechniquesPage>();
            builder.Services.AddTransient<TechniquePage>();
            builder.Services.AddTransient<Views.MeditacaoGuiadaPage>();
            builder.Services.AddTransient<ChallengesPage>();
            builder.Services.AddTransient<ChallengeLockedPage>();
            builder.Services.AddTransient<ChallengeActivePage>();
            builder.Services.AddTransient<ChallengeDonePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<PrivacyPage>();

            return builder.Build();
        }
    }
}
