namespace Social_Anxiety.Controls;

public enum NavTab { None, Home, History, Techniques, Challenges, Profile }

/// <summary>Barra inferior das abas. Cada página define a aba ativa no XAML (<c>Active="History"</c>).</summary>
public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveProperty = BindableProperty.Create(
        nameof(Active), typeof(NavTab), typeof(BottomNavBar), NavTab.None,
        propertyChanged: (bindable, _, _) => ((BottomNavBar)bindable).UpdateItems());

    // Uma navegação por vez no app todo: cada página tem a sua barra, então a trava é estática.
    private static bool navigating;

    private readonly (Button Item, NavTab Tab, string Route, string Label)[] items;

    public BottomNavBar()
    {
        InitializeComponent();
        items =
        [
            (HomeItem, NavTab.Home, "//home", "Início"),
            (HistoryItem, NavTab.History, "//home/history", "Histórico"),
            (TechniquesItem, NavTab.Techniques, "//home/library", "Técnicas"),
            (ChallengesItem, NavTab.Challenges, "//challenges", "Desafios"),
            (ProfileItem, NavTab.Profile, "//profile", "Perfil"),
        ];
        UpdateItems();
    }

    public NavTab Active
    {
        get => (NavTab)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    private void UpdateItems()
    {
        var normal = (Style)Resources["NavItem"];
        var active = (Style)Resources["NavItemActive"];
        foreach (var (item, tab, _, label) in items)
        {
            var current = tab == Active;
            item.Style = current ? active : normal;
            SemanticProperties.SetDescription(item, current ? $"{label}, aba atual" : label);
        }
    }

    private async void OnItemClicked(object? sender, EventArgs e)
    {
        var target = Array.Find(items, i => i.Item == sender);
        if (navigating || target.Tab == Active) return;
        navigating = true;
        try { await Shell.Current.GoToAsync(target.Route); }
        finally { navigating = false; }
    }
    private async void OnConfiguracoesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settings");
    }
}
