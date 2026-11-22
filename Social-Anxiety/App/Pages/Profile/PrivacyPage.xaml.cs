namespace Social_Anxiety;

public partial class PrivacyPage : ContentPage
{
    public PrivacyPage() => InitializeComponent();

    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
