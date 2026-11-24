namespace Social_Anxiety;

public partial class BreathePage : ContentPage
{
    private readonly BreathingSession session = new();
    private readonly IDispatcherTimer timer;
    private bool initializing = true;
    private bool navigating;
    private Window? observedWindow;
    private BreathingPhase? announcedPhase;
    private bool systemReducesMotion;
#if WINDOWS
    private Microsoft.UI.Windowing.AppWindow? nativeWindow;
#endif

    public BreathePage()
    {
        InitializeComponent();
        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += OnFrame;
        SkipHoldSwitch.IsToggled = Preferences.Default.Get("breathing-skip-hold", true);
        systemReducesMotion = SystemReducesMotion;
        ReducedMotionSwitch.IsToggled = Preferences.Default.Get("breathing-reduced-motion", false) || systemReducesMotion;
        session.SkipHold = SkipHoldSwitch.IsToggled;
        initializing = false;
        Render();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        systemReducesMotion = SystemReducesMotion;
        observedWindow = Window ?? Application.Current?.Windows.FirstOrDefault();
        if (observedWindow is not null)
        {
#if !WINDOWS
            // No Windows, Deactivated é só perda de foco (a prática continua visível); minimizar e ocultar são tratados abaixo.
            observedWindow.Deactivated += OnWindowInactive;
#endif
            observedWindow.Stopped += OnWindowInactive;
#if WINDOWS
            nativeWindow = (observedWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window)?.AppWindow;
            if (nativeWindow is not null) nativeWindow.Changed += OnNativeWindowChanged;
#endif
        }
        Render();
    }

    protected override void OnDisappearing()
    {
        Pause();
#if WINDOWS
        if (nativeWindow is not null) nativeWindow.Changed -= OnNativeWindowChanged;
        nativeWindow = null;
#endif
        if (observedWindow is not null)
        {
#if !WINDOWS
            observedWindow.Deactivated -= OnWindowInactive;
#endif
            observedWindow.Stopped -= OnWindowInactive;
            observedWindow = null;
        }
        base.OnDisappearing();
    }

    private static bool SystemReducesMotion
    {
        get
        {
#if WINDOWS
            return !new Windows.UI.ViewManagement.UISettings().AnimationsEnabled;
#elif ANDROID
            return OperatingSystem.IsAndroidVersionAtLeast(26) && !Android.Animation.ValueAnimator.AreAnimatorsEnabled();
#elif IOS || MACCATALYST
            return UIKit.UIAccessibility.IsReduceMotionEnabled;
#else
            return false;
#endif
        }
    }

    private void OnWindowInactive(object? sender, EventArgs e) => Pause();
#if WINDOWS
    private void OnNativeWindowChanged(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (!sender.IsVisible || sender.Presenter is Microsoft.UI.Windowing.OverlappedPresenter { State: Microsoft.UI.Windowing.OverlappedPresenterState.Minimized })
            Pause();
    }
#endif
    private void OnFrame(object? sender, EventArgs e)
    {
#if WINDOWS
        if ((observedWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window)?.AppWindow.Presenter
            is Microsoft.UI.Windowing.OverlappedPresenter { State: Microsoft.UI.Windowing.OverlappedPresenterState.Minimized })
        {
            Pause();
            return;
        }
#endif
        Render();
    }

    private void Pause()
    {
        session.Pause();
        timer.Stop();
        Render();
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        if (session.IsRunning) Pause();
        else
        {
            session.Start();
            systemReducesMotion = SystemReducesMotion;
            timer.Interval = TimeSpan.FromMilliseconds(ReducedMotionSwitch.IsToggled || systemReducesMotion ? 100 : 16);
            timer.Start();
            Render();
        }
    }

    private void Render()
    {
        var reduced = ReducedMotionSwitch.IsToggled || systemReducesMotion;
        var phase = session.Phase;
        BreathCircle.Scale = reduced ? 0.85 : session.Scale;
        BreathHalo.Scale = reduced ? 1 : Math.Min(1, session.Scale + 0.14);
        BreathHalo.Opacity = reduced ? 0.5 : 0.35 + (session.Scale - 0.64) * 0.8;
        ToggleButton.Text = session.IsRunning ? "Pausar" : session.HasStarted ? "Continuar" : "Começar";
        FinishButton.IsVisible = session.HasStarted;
        Countdown.Text = session.HasStarted ? $"{session.SecondsRemaining} s" : "Calme";
        ElapsedLabel.Text = session.HasStarted ? $"Tempo de prática: {(int)session.Elapsed.TotalMinutes:00}:{session.Elapsed.Seconds:00}" : "";
        PhaseLabel.Text = !session.HasStarted ? "Quando quiser, comece." : !session.IsRunning ? "Pausa" : phase switch
        {
            BreathingPhase.Inhale => "Inspire", BreathingPhase.Hold => "Segure", _ => "Expire"
        };
        PhaseCaption.Text = !session.HasStarted
            ? session.SkipHold ? "Inspire 4 s · expire 6 s" : "Inspire 4 s · pausa opcional 2 s · expire 6 s"
            : !session.IsRunning ? "Retome quando for confortável, sem pressa."
            : phase switch
            {
                BreathingPhase.Inhale => "Deixe o ar entrar suavemente.",
                BreathingPhase.Hold => "Pausa opcional, apenas se confortável.",
                _ => "Solte o ar devagar, sem forçar."
            };
        if (session.IsRunning && announcedPhase != phase)
        {
            announcedPhase = phase;
            SemanticScreenReader.Announce(PhaseLabel.Text);
        }
    }

    private void OnSkipHoldToggled(object? sender, ToggledEventArgs e)
    {
        if (initializing) return;
        Pause();
        session.SkipHold = e.Value;
        session.Reset();
        announcedPhase = null;
        Preferences.Default.Set("breathing-skip-hold", e.Value);
        Render();
    }

    private void OnReducedMotionToggled(object? sender, ToggledEventArgs e)
    {
        if (initializing) return;
        Preferences.Default.Set("breathing-reduced-motion", e.Value);
        timer.Interval = TimeSpan.FromMilliseconds(e.Value || systemReducesMotion ? 100 : 16);
        Render();
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        if (navigating || !session.HasStarted) return;
        navigating = true;
        Pause();
        try
        {
            await Shell.Current.GoToAsync("../breatheFinish");
        }
        finally { navigating = false; }
    }

    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
