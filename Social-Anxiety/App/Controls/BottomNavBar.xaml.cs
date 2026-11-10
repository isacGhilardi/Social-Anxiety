namespace Social_Anxiety.Controls;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnDesafiosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//challenges");
    }
}