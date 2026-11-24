// Alias: "Path" sozinho conflita com System.IO.Path (ImplicitUsings).
using ShapePath = Microsoft.Maui.Controls.Shapes.Path;

namespace Social_Anxiety.Controls;

public enum NavTab { None, Home, History, Techniques, Challenges, Profile }

/// <summary>
/// Barra inferior das abas. Cada página define a aba ativa no XAML (<c>Active="History"</c>).
/// Cada tela tem a sua instância da barra; para dar continuidade, a barra da tela nova faz o indicador
/// deslizar a partir da aba que foi tocada na tela anterior. A troca de tela não tem animação de empilhar.
/// </summary>
public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveProperty = BindableProperty.Create(
        nameof(Active), typeof(NavTab), typeof(BottomNavBar), NavTab.None,
        propertyChanged: (bindable, _, _) => ((BottomNavBar)bindable).OnActiveChanged());

    private const uint SlideMilliseconds = 260;
    private const long SlideWindowMilliseconds = 1500;

    // Uma navegação por vez no app todo: cada página tem a sua barra, então a trava é estática.
    private static bool navigating;
    // Aba de onde saiu o último toque. A barra da tela seguinte desliza o indicador a partir dela.
    private static NavTab slideFrom = NavTab.None;
    private static long slideFromTicks;

    private readonly (Button Item, ShapePath Icon, Label Text, NavTab Tab, string Route, string Label)[] items;
    private Page? page;
    private double placedWidth = -1;
    private int placedIndex = -1;

    public BottomNavBar()
    {
        InitializeComponent();
        items =
        [
            (HomeItem, HomeIcon, HomeLabel, NavTab.Home, "//home", "Início"),
            (HistoryItem, HistoryIcon, HistoryLabel, NavTab.History, "//home/history", "Histórico"),
            (TechniquesItem, TechniquesIcon, TechniquesLabel, NavTab.Techniques, "//home/library", "Técnicas"),
            (ChallengesItem, ChallengesIcon, ChallengesLabel, NavTab.Challenges, "//challenges", "Desafios"),
            (ProfileItem, ProfileIcon, ProfileLabel, NavTab.Profile, "//profile", "Perfil"),
        ];
        UpdateItems();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ItemsGrid.SizeChanged += (_, _) => PlaceIndicator();
    }

    public NavTab Active
    {
        get => (NavTab)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    private static int IndexOf(NavTab tab) => tab == NavTab.None ? -1 : (int)tab - 1;

    private void OnActiveChanged()
    {
        UpdateItems();
        placedIndex = -1;
        PlaceIndicator();
    }

    private void UpdateItems()
    {
        var icon = (Style)Resources["NavIcon"];
        var iconActive = (Style)Resources["NavIconActive"];
        var label = (Style)Resources["NavLabel"];
        var labelActive = (Style)Resources["NavLabelActive"];
        foreach (var (item, path, text, tab, _, name) in items)
        {
            var current = tab == Active;
            path.Style = current ? iconActive : icon;
            text.Style = current ? labelActive : label;
            // O teste funcional lê este nome ("Histórico" ou "Histórico, aba atual").
            SemanticProperties.SetDescription(item, current ? $"{name}, aba atual" : name);
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        page = FindPage();
        if (page is not null) page.Appearing += OnPageAppearing;
        PlaceIndicator();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (page is not null) page.Appearing -= OnPageAppearing;
        page = null;
    }

    // Ao voltar para uma tela que já estava aberta (por exemplo, o Início), o indicador também desliza.
    private void OnPageAppearing(object? sender, EventArgs e) => PlaceIndicator();

    private Page? FindPage()
    {
        Element? element = Parent;
        while (element is not null and not Page) element = element.Parent;
        return element as Page;
    }

    // Chamado por SizeChanged, Loaded e Appearing. Só reposiciona quando há um deslize pendente ou quando
    // a aba ou a largura mudaram; assim uma chamada seguinte não interrompe o deslize que já começou.
    private void PlaceIndicator()
    {
        var index = IndexOf(Active);
        Indicator.IsVisible = index >= 0;
        var width = ItemsGrid.Width - ItemsGrid.Padding.HorizontalThickness;
        if (index < 0 || width <= 0) return;

        var from = TakeSlideFrom(index);
        var unchanged = index == placedIndex && Math.Abs(width - placedWidth) < 0.5;
        if (from < 0 && unchanged) return;
        placedIndex = index;
        placedWidth = width;

        var column = width / items.Length;
        var target = index * column;
        Indicator.CancelAnimations();
        if (from >= 0 && from != index && !AppPreferences.ReduceMotion)
        {
            Indicator.TranslationX = from * column;
            _ = Indicator.TranslateToAsync(target, 0, SlideMilliseconds, Easing.CubicOut);
        }
        else
        {
            Indicator.TranslationX = target;
        }
    }

    // Devolve a aba de onde veio o toque (ou -1) e a consome. A barra da própria aba de origem não consome,
    // para o caso de a tela que está saindo receber um evento durante a navegação.
    private static int TakeSlideFrom(int ownIndex)
    {
        var recent = Environment.TickCount64 - slideFromTicks < SlideWindowMilliseconds;
        var from = recent ? IndexOf(slideFrom) : -1;
        if (from == ownIndex) return -1;
        slideFrom = NavTab.None;
        return from;
    }

    private async void OnItemClicked(object? sender, EventArgs e)
    {
        var target = Array.Find(items, i => i.Item == sender);
        if (navigating || target.Item is null || target.Tab == Active) return;
        navigating = true;
        slideFrom = Active;
        slideFromTicks = Environment.TickCount64;
        try { await Shell.Current.GoToAsync(target.Route, false); }
        finally { navigating = false; }
    }
}
