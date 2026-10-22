namespace Social_Anxiety;

public partial class BreatheFinishPage : ContentPage
{
    private bool navigating;
    public BreatheFinishPage() => InitializeComponent();

    private async void OnReturnClicked(object? sender, EventArgs e)
    {
        if (navigating) return;
        navigating = true;
        try { await Navigation.PopAsync(false); }
        finally { navigating = false; }
    }

    private async void OnAgainClicked(object? sender, EventArgs e)
    {
        if (navigating) return;
        navigating = true;
        try
        {
            await Shell.Current.GoToAsync("../breathe");
        }
        finally { navigating = false; }
    }

    private async void OnHelpClicked(object? sender, EventArgs e) => await App.OpenHelp();
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
