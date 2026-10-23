using Microsoft.Maui.Controls;
using Social_Anxiety.Core.Auth;

namespace Social_Anxiety;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingState state;
    private readonly SessionState session;
    private readonly string route;
    private bool navigating;

    public string MotiveGroup { get; } = Guid.NewGuid().ToString();

    // internal: a injeção de dependência só enxerga o construtor público, que recebe o estado compartilhado.
    internal OnboardingPage(OnboardingState state, SessionState session, string route)
    {
        this.state = state;
        this.session = session;
        this.route = route;
        InitializeComponent();
        BindingContext = this;
    }

    public OnboardingPage(OnboardingState state, SessionState session)
        : this(state, session, "welcome")
    {
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (IsAccess) Preferences.Default.Set(App.OnboardingDoneKey, true);
        OnPropertyChanged(nameof(CanGoBack));
    }

    public bool IsWelcome => route == "welcome";
    public bool IsPurpose => route == "purpose";
    public bool IsBoundary => route == "boundary";
    public bool IsMotive => route == "motive";
    public bool IsAccess => route == "access";
    public bool IsSupport => route == "support";
    public bool IsHelp => route == "help";
    public bool IsPlaceholder => !(IsWelcome || IsPurpose || IsBoundary ||
                                   IsMotive || IsAccess || IsSupport || IsHelp);
    public bool CanGoBack => !IsWelcome && Navigation.NavigationStack.Count > 1;

    public string HeaderTitle => route switch
    {
        "motive" => "Vamos começar",
        "access" => "Seu espaço",
        "help" => "Buscar ajuda",
        "support" => "Uma pausa agora",
        _ => "Calme"
    };

    public string PlaceholderTitle => route switch
    {
        "login" => "Entrar",
        "signup" => "Criar conta",
        "checkin" => "Seu check-in",
        "challenges" => "Pequenos passos",
        "home" => "Seu espaço",
        "breathe" => "Respiração guiada",
        "stop" => "Técnica STOP",
        "acalme" => "ACALME-SE",
        _ => "Calme"
    };

    public string SupportText => IsSupport
        ? "Voltar ao que eu estava fazendo"
        : "Preciso de apoio agora";

    public bool CanContinue => state.Motive is not null;
    public string ContinueText => CanContinue ? "Continuar" : "Escolha uma opção";
    public bool IsFeelingsSelected => state.Motive == "Entender meus sentimentos";
    public bool IsAnxietySelected => state.Motive == "Aprender a lidar com a ansiedade";
    public bool IsSocialSelected => state.Motive == "Dar pequenos passos sociais";

    private void OnMotiveCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!IsMotive || !e.Value ||
            sender is not RadioButton { Value: string motive })
            return;

        state.Motive = motive;

        foreach (var property in new[]
        {
            nameof(CanContinue), nameof(ContinueText),
            nameof(IsFeelingsSelected), nameof(IsAnxietySelected),
            nameof(IsSocialSelected)
        })
            OnPropertyChanged(property);
    }

    private async void OnNavigateClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string destination })
            await Go(destination);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync(false);
    }

    private async void OnSupportClicked(object? sender, EventArgs e)
    {
        if (IsSupport)
        {
            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync(false);
        }
        else
            await Go("support");
    }

    private async Task Go(string destination)
    {
        if (navigating) return;

        navigating = true;
        try
        {
            if (destination == "guest")
            {
                await session.StartGuestAsync();
                await Shell.Current.GoToAsync("//home");
            }
            else if (destination == "home")
                await Shell.Current.GoToAsync("//home");
            else if (destination is "checkin" or "breathe")
                await Shell.Current.GoToAsync(destination);
            else
                await Navigation.PushAsync(new OnboardingPage(state, session, destination), false);
        }
        finally
        {
            navigating = false;
        }
    }
}
