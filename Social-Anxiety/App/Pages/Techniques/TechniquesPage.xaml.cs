using Social_Anxiety.Core.Techniques;

namespace Social_Anxiety;

public partial class TechniquesPage : ContentPage
{
    private bool navigating;

    public TechniquesPage()
    {
        InitializeComponent();
        foreach (var technique in TechniqueCatalog.All)
        {
            var button = new Button
            {
                Text = technique.Name,
                AutomationId = $"techniques-{technique.Key}",
                Style = (Style)Application.Current!.Resources["CalmeButton"],
                CommandParameter = technique.Key
            };
            SemanticProperties.SetDescription(button, $"{technique.Name}. {technique.Description}");
            button.Clicked += OnTechniqueClicked;
            TechniqueCards.Children.Add(button);
        }
    }

    private async void OnTechniqueClicked(object? sender, EventArgs e)
    {
        if (navigating || sender is not Button { CommandParameter: string key }) return;
        navigating = true;
        try { await Shell.Current.GoToAsync($"technique?key={Uri.EscapeDataString(key)}"); }
        finally { navigating = false; }
    }

    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
