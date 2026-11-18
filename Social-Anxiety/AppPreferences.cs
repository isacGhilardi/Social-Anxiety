namespace Social_Anxiety;

public enum ThemePreference { System, Light, Dark }

/// <summary>
/// Preferências do app: tema e "reduzir movimentos". A tela de configurações (Esaú) só lê e grava aqui.
/// </summary>
public static class AppPreferences
{
    private const string ThemeKey = "calme.theme";
    // Mesma chave que a respiração guiada já usa, então a BreathePage continua respeitando a opção.
    private const string ReduceMotionKey = "breathing-reduced-motion";

    public static bool ReduceMotion
    {
        get => Preferences.Default.Get(ReduceMotionKey, false);
        set => Preferences.Default.Set(ReduceMotionKey, value);
    }

    public static ThemePreference Theme
    {
        get => Enum.TryParse<ThemePreference>(Preferences.Default.Get(ThemeKey, ""), out var theme) ? theme : ThemePreference.System;
        set
        {
            Preferences.Default.Set(ThemeKey, value.ToString());
            ApplyTheme();
        }
    }

    /// <summary>Aplica o tema salvo. Chamar uma vez ao abrir o app.</summary>
    public static void ApplyTheme()
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = Theme switch
        {
            ThemePreference.Light => AppTheme.Light,
            ThemePreference.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
