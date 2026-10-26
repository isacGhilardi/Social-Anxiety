using Social_Anxiety.Core.Techniques;

namespace Social_Anxiety;

public partial class TechniquePage : ContentPage, IQueryAttributable
{
    private bool navigating;

    public TechniquePage() => InitializeComponent();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var technique = TechniqueCatalog.Find(query.TryGetValue("key", out var key) ? key as string : null);
        PracticeContent.IsVisible = technique is not null;
        MissingMessage.IsVisible = technique is null;
        FinishContent.IsVisible = false;
        if (technique is null) return;

        Title = technique.Name;
        TechniqueTag.Text = technique.Tag;
        TechniqueTitle.Text = technique.Title;
        TechniqueDescription.Text = technique.Description;
        var resources = Application.Current!.Resources;
        string badgeColor = technique.Key == "acalme" ? "CalmeLilac" : "CalmeMint";
        TechniqueBadge.SetAppThemeColor(BackgroundColorProperty,
            (Color)resources[badgeColor], (Color)resources[badgeColor + "Dark"]);
        BindableLayout.SetItemsSource(StepsList, technique.Steps);
        SemanticProperties.SetDescription(StepsList, $"{technique.Name}, {technique.Steps.Count} passos");
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        PracticeContent.IsVisible = false;
        FinishContent.IsVisible = true;
        await ContentScroll.ScrollToAsync(0, 0, false);
        SemanticScreenReader.Default.Announce("Uma pausa conta. Sua prática foi concluída.");
    }

    private async void OnLibraryClicked(object? sender, EventArgs e)
    {
        if (navigating) return;
        navigating = true;
        try
        {
            // Ao voltar à biblioteca já aberta, não empilha outra cópia.
            if (Navigation.NavigationStack.Count > 1 && Navigation.NavigationStack[^2] is TechniquesPage)
                await Shell.Current.GoToAsync("..");
            else
                await Shell.Current.GoToAsync("library");
        }
        finally { navigating = false; }
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        if (navigating) return;
        navigating = true;
        try { await Shell.Current.GoToAsync("//home"); }
        finally { navigating = false; }
    }

    private async void OnHelpClicked(object? sender, EventArgs e) => await App.OpenHelp();
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
