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
            // Páginas registradas para o Shell criá-las pela injeção de dependência.
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<AccessPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<LoginPage>();

            return builder.Build();
        }
    }
}
