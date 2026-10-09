using Microsoft.Maui.Controls.Shapes;

namespace Social_Anxiety;

public sealed class OnboardingState
{
    public string? Motive { get; set; }
}

public sealed class OnboardingPage : ContentPage
{
    private readonly OnboardingState state;
    private readonly string route;
    private readonly VerticalStackLayout body = new() { Spacing = 16, Padding = new Thickness(24, 8, 24, 28) };
    private readonly List<RadioButton> choices = [];
    private Button? continueButton;
    private bool navigating;

    public OnboardingPage(OnboardingState state, string route)
    {
        this.state = state;
        this.route = route;
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetTabBarIsVisible(this, false);
        SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
        Theme(this, BackgroundColorProperty, "Bg");

        var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)), Padding = new Thickness(18, 8), ColumnSpacing = 6 };
        var back = Action(route == "welcome" ? "❧" : "‹", async () => { if (Navigation.NavigationStack.Count > 1) await Navigation.PopAsync(false); }, true);
        back.WidthRequest = 48;
        back.FontSize = 28;
        back.IsVisible = route != "welcome";
        SemanticProperties.SetDescription(back, "Voltar");
        header.Add(back);
        var brand = Text(route == "motive" ? "Vamos começar" : route == "access" ? "Seu espaço" : route == "help" ? "Buscar ajuda" : route == "support" ? "Uma pausa agora" : "Calme", 22, true);
        brand.VerticalOptions = LayoutOptions.Center;
        header.Add(brand, 1);

        var support = Action(route == "support" ? "Voltar ao que eu estava fazendo" : "Preciso de apoio agora", async () =>
        {
            if (route == "support") await Navigation.PopAsync(false);
            else await Go("support");
        });
        Theme(support, BackgroundColorProperty, "Mint");
        Theme(support, Button.TextColorProperty, "Teal");
        support.CornerRadius = 99;
        support.MinimumHeightRequest = 44;
        support.Padding = new Thickness(12, 10);
        var footer = new VerticalStackLayout { Padding = new Thickness(24, 8, 24, 16), Children = { support } };
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)) };
        layout.Add(header);
        layout.Add(new ScrollView { Content = body }, 0, 1);
        layout.Add(footer, 0, 2);
        Content = layout;
        Build();
    }

    private void Build()
    {
        switch (route)
        {
            case "welcome":
                Badge("UM ESPAÇO PARA VOCÊ", true); Art("leaf", false);
                Heading("Mais calma.\nNo seu tempo.", 34);
                Copy("Conheça suas emoções e encontre pequenas formas de lidar com a ansiedade social.");
                Dots(0); Next("Conhecer o Calme", "purpose"); Next("Já tenho uma conta", "login", true);
                break;
            case "purpose":
                Badge("PERCEBER · ACOLHER · PRATICAR"); Art("wind");
                Heading("Um pequeno cuidado,\num dia de cada vez.");
                Copy("Registre como você se sente, perceba padrões e pratique no seu ritmo.");
                LinkCard("Seu check-in emocional", "Um minuto para olhar para você", "checkin", "heart");
                LinkCard("Pequenos passos", "Práticas e desafios que você escolhe", "challenges", "path");
                Dots(1); Next("Continuar", "boundary"); break;
            case "boundary":
                Art("heart"); Heading("Cuidado também é\npedir ajuda.");
                Card("O Calme não faz diagnósticos", "Este app não substitui acompanhamento profissional.", "Mint");
                Copy("Se a ansiedade trouxer sofrimento ou atrapalhar sua rotina, procure um profissional de saúde.");
                Dots(2); Next("Entendi, vamos continuar", "motive"); Next("Quando buscar ajuda?", "help", true); break;
            case "motive":
                Badge("COMECE DO SEU JEITO"); Heading("O que te trouxe\naté aqui?");
                Copy("Escolha o que faz sentido para você agora. Você pode mudar depois.");
                foreach (var motive in new[] { "Entender meus sentimentos", "Aprender a lidar com a ansiedade", "Dar pequenos passos sociais" })
                {
                    var choice = new RadioButton { Content = motive, GroupName = "Motive", Value = motive, FontFamily = "NunitoExtraBold", FontSize = 16, MinimumHeightRequest = 64, Padding = 16, IsChecked = state.Motive == motive };
                    Theme(choice, RadioButton.TextColorProperty, "Text");
                    Theme(choice, BackgroundColorProperty, choice.IsChecked ? "Blue" : "Surface");
                    choice.CheckedChanged += (_, args) =>
                    {
                        if (args.Value) state.Motive = motive;
                        foreach (var item in choices) Theme(item, BackgroundColorProperty, item.IsChecked ? "Blue" : "Surface");
                        UpdateContinue();
                    };
                    var outline = new Border { Content = choice, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 } };
                    Theme(outline, Border.StrokeProperty, "Border");
                    choices.Add(choice); body.Add(outline);
                }
                Dots(3); continueButton = Action("Escolha uma opção", () => Go("access")); body.Add(continueButton); UpdateContinue();
                Next("Prefiro explorar primeiro", "access", true); break;
            case "access":
                Art("leaf"); Heading("Este espaço é seu.");
                Copy("Crie uma conta para continuar seu cuidado ou explore como convidado.");
                Next("Criar minha conta", "signup"); Next("Já tenho uma conta", "login", true); Next("Continuar como convidado", "home", true);
                Card("No seu ritmo", "Você não precisa de uma conta para usar as técnicas de apoio.", "Mint"); break;
            case "support":
                Badge("VOCÊ PODE FAZER UMA PAUSA", true); Heading("Vamos com calma,\num instante de cada vez.");
                Copy("Escolha uma prática que pareça confortável. Você pode trocar ou parar quando quiser.");
                LinkCard("Respiração guiada", "Acompanhe um ritmo suave", "breathe", "wind");
                LinkCard("STOP", "Pare, respire, observe e prossiga", "stop", "leaf");
                LinkCard("ACALME-SE", "Acolha o que sente, passo a passo", "acalme", "heart");
                Copy("Se precisar de alguém, buscar ajuda também é uma forma de cuidado."); Next("Buscar ajuda profissional", "help"); break;
            case "help":
                Badge("VOCÊ NÃO PRECISA LIDAR SOZINHO(A)", true); Heading("Buscar ajuda é\num cuidado com você.");
                Copy("Você pode procurar um profissional em qualquer momento. Não é preciso esperar a situação piorar.");
                Card("Quando considerar apoio?", "Quando o sofrimento se repete, você evita situações importantes ou a ansiedade interfere no estudo, trabalho, sono ou relações.");
                Card("Converse com um profissional", "Um psicólogo ou médico pode ouvir sua história e orientar os próximos passos.");
                Card("Procure caminhos de acesso", "Uma Unidade Básica de Saúde (UBS) pode orientar sobre os serviços de saúde disponíveis na sua região.");
                Card("CVV · apoio emocional · 188", "Atendimento gratuito, 24 horas por dia. Uma escuta quando você precisa conversar.", "Blue");
                Card("Se houver risco imediato", "Ligue para o SAMU 192 ou procure um serviço de urgência. Se possível, peça companhia a alguém de confiança.\n\nO CVV oferece apoio emocional e não substitui atendimento de emergência.");
                Copy("Este app não substitui acompanhamento profissional e não realiza diagnósticos."); break;
            default:
                Heading(route switch { "login" => "Entrar", "signup" => "Criar conta", "checkin" => "Seu check-in", "challenges" => "Pequenos passos", "home" => "Seu espaço", "breathe" => "Respiração guiada", "stop" => "Técnica STOP", "acalme" => "ACALME-SE", _ => "Calme" });
                Copy("Esta funcionalidade ainda não foi implementada nesta versão do Calme.");
                body.Add(Action("Voltar", async () => await Navigation.PopAsync(false), true)); break;
        }
    }

    private void UpdateContinue()
    {
        if (continueButton is null) return;
        continueButton.IsEnabled = state.Motive is not null;
        continueButton.Text = state.Motive is null ? "Escolha uma opção" : "Continuar";
    }

    private async Task Go(string destination)
    {
        if (navigating) return;
        navigating = true;
        try { await Navigation.PushAsync(new OnboardingPage(state, destination), false); }
        finally { navigating = false; }
    }

    private static void Theme(BindableObject view, BindableProperty property, string token)
    {
        var light = (Color)Application.Current!.Resources["Calme" + token];
        var dark = (Color)Application.Current.Resources["Calme" + token + "Dark"];
        if (property == Border.StrokeProperty)
            view.SetAppTheme<Brush>(property, new SolidColorBrush(light), new SolidColorBrush(dark));
        else
            view.SetAppThemeColor(property, light, dark);
    }

    private static Label Text(string value, double size = 16, bool bold = false, string color = "Text")
    {
        var label = new Label { Text = value, FontSize = size, FontFamily = bold ? "NunitoExtraBold" : "Nunito", LineHeight = 1.25 };
        Theme(label, Label.TextColorProperty, color); return label;
    }

    private void Heading(string title, double size = 28)
    {
        var label = Text(title, size, true); label.CharacterSpacing = -0.5;
        SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level1); body.Add(label);
    }
    private void Copy(string text) => body.Add(Text(text, color: "Muted"));
    private void Next(string title, string destination, bool link = false) => body.Add(Action(title, () => Go(destination), link));
    private static Button Action(string title, Func<Task> action, bool link = false)
    {
        var button = new Button { Text = title, Style = (Style)Application.Current!.Resources[link ? "CalmeTextButton" : "CalmeButton"] };
        button.Clicked += async (_, _) => await action(); return button;
    }
    private void Badge(string title, bool mint = false)
    {
        var label = Text(title, 12, true, mint ? "Teal" : "Primary"); label.CharacterSpacing = 0.3;
        var border = new Border { Content = label, Padding = new Thickness(12, 6), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 99 }, HorizontalOptions = LayoutOptions.Start };
        Theme(border, BackgroundColorProperty, mint ? "Mint" : "Blue"); body.Add(border);
    }
    private void Card(string title, string description, string color = "Surface")
    {
        var stack = new VerticalStackLayout { Spacing = 12, Children = { Text(title, 22, true), Text(description, color: "Muted") } };
        var card = new Border { Content = stack, Padding = 20, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 24 } };
        Theme(card, BackgroundColorProperty, color); body.Add(card);
    }
    private void LinkCard(string title, string description, string destination, string icon)
    {
        var content = new Grid { ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)), ColumnSpacing = 12, Padding = 16, InputTransparent = true };
        var image = new Border { WidthRequest = 40, HeightRequest = 40, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, VerticalOptions = LayoutOptions.Center, Content = new Image { Source = icon + ".png", WidthRequest = 24, HeightRequest = 24 } };
        Theme(image, BackgroundColorProperty, "Blue"); content.Add(image);
        var labels = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { Text(title, 18, true), Text(description, 14, color: "Muted") } };
        content.Add(labels, 1); content.Add(Text("›", 24, color: "Muted"), 2);
        var button = Action("", () => Go(destination), true);
        SemanticProperties.SetDescription(button, title + ". " + description);
        var grid = new Grid { MinimumHeightRequest = 88, Children = { content, button } };
        var card = new Border { Content = grid, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 24 } };
        Theme(card, BackgroundColorProperty, "Surface"); body.Add(card);
    }
    private void Art(string icon, bool compact = true)
    {
        var size = compact ? 148 : 198;
        var seed = new Border { WidthRequest = compact ? 116 : 156, HeightRequest = compact ? 116 : 156, StrokeThickness = 0, StrokeShape = new Ellipse(), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Content = new Image { Source = icon + ".png", WidthRequest = 66, HeightRequest = 66 } };
        Theme(seed, BackgroundColorProperty, "Mint");
        var ripple = new Border { Content = seed, WidthRequest = size, HeightRequest = size, StrokeShape = new Ellipse(), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        Theme(ripple, BackgroundColorProperty, "Blue"); Theme(ripple, Border.StrokeProperty, "Track");
        body.Add(new Grid { HeightRequest = compact ? 156 : 204, Children = { ripple } });
    }
    private void Dots(int active)
    {
        var dots = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, Padding = new Thickness(0, 3) };
        SemanticProperties.SetDescription(dots, $"Passo {active + 1} de 4");
        for (var i = 0; i < 4; i++)
        {
            var dot = new Border { WidthRequest = i == active ? 24 : 8, HeightRequest = 8, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 99 } };
            Theme(dot, BackgroundColorProperty, i == active ? "Primary" : "Track"); dots.Add(dot);
        }
        body.Add(dots);
    }
}
