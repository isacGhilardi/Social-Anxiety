using Microsoft.Maui.Controls.Shapes;
using Social_Anxiety.Core.Techniques;
// Alias: "Path" sozinho conflita com System.IO.Path (ImplicitUsings).
using ShapePath = Microsoft.Maui.Controls.Shapes.Path;

namespace Social_Anxiety;

public partial class TechniquesPage : ContentPage
{
    private const string LeafIcon = "M5,19 C5,11 10,5 20,4 C19,14 13,19 5,19 Z M5,19 L12,12";
    private const string PauseIcon = "M12,21 A9,9 0 1 0 12,3 A9,9 0 1 0 12,21 Z M10,9 L10,15 M14,9 L14,15";
    private const string HeartIcon = "M12,20 C12,20 5,15.5 5,10 A4,4 0 0 1 12,7.4 A4,4 0 0 1 19,10 C19,15.5 12,20 12,20 Z";
    private const string ChevronIcon = "M9,6 L15,12 L9,18";

    private static readonly PathGeometryConverter GeometryConverter = new();
    private bool navigating;

    public TechniquesPage()
    {
        InitializeComponent();
        foreach (var technique in TechniqueCatalog.All)
            TechniqueCards.Children.Add(BuildCard(technique));
    }

    // Cartão inteiro tocável: o conteúdo é visual (InputTransparent) e o toque vai num Button transparente por cima,
    // que mantém o AutomationId techniques-{key} e a descrição lida pelo leitor de tela.
    private View BuildCard(TechniqueContent technique)
    {
        var resources = Application.Current!.Resources;
        var (iconData, boxStyle, iconStyle) = technique.Key switch
        {
            "mindfulness" => (LeafIcon, "FlowIconBoxMint", "FlowIconMint"),
            "acalme" => (HeartIcon, "FlowIconBoxLilac", "FlowIconLilac"),
            "stop" => (PauseIcon, "FlowIconBox", "FlowIcon"),
            _ => (LeafIcon, "FlowIconBox", "FlowIcon"),
        };

        var iconBox = new Border
        {
            Style = (Style)resources[boxStyle],
            Content = new ShapePath { Style = (Style)resources[iconStyle], Data = ToGeometry(iconData) },
        };

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        texts.Children.Add(new Label { Text = technique.Name, Style = (Style)resources["FlowCardTitle"] });
        texts.Children.Add(new Label { Text = technique.Tag, Style = (Style)resources["FlowEyebrow"] });

        var chevron = new ShapePath { Style = (Style)resources["FlowIconMuted"], Data = ToGeometry(ChevronIcon) };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 12,
        };
        row.Add(iconBox, 0);
        row.Add(texts, 1);
        row.Add(chevron, 2);

        var card = new Border { Style = (Style)resources["FlowListCard"], Content = row, InputTransparent = true };

        var button = new Button
        {
            AutomationId = $"techniques-{technique.Key}",
            Style = (Style)resources["CalmeOverlayButton"],
            CommandParameter = technique.Key,
        };
        SemanticProperties.SetDescription(button, $"{technique.Name}. {technique.Description}");
        button.Clicked += OnTechniqueClicked;

        var cell = new Grid();
        cell.Add(card);
        cell.Add(button);
        return cell;
    }

    private static Geometry ToGeometry(string data) => (Geometry)GeometryConverter.ConvertFromInvariantString(data)!;

    private async void OnTechniqueClicked(object? sender, EventArgs e)
    {
        if (navigating || sender is not Button { CommandParameter: string key }) return;
        navigating = true;
        try { await Shell.Current.GoToAsync($"technique?key={Uri.EscapeDataString(key)}"); }
        finally { navigating = false; }
    }

    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
