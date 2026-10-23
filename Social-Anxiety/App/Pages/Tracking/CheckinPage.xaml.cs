using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class CheckinPage : ContentPage
{
    private readonly CheckInService checkIns;
    private int score;
    private bool saving;

    public CheckinPage(CheckInService checkIns)
    {
        this.checkIns = checkIns;
        InitializeComponent();
    }

    private void OnScoreClicked(object? sender, EventArgs e)
    {
        if (saving || sender is not Button button || !int.TryParse(button.CommandParameter?.ToString(), out var value) || value is < 1 or > 5) return;
        score = value;
        var choices = new[] { Score1, Score2, Score3, Score4, Score5 };
        for (var i = 0; i < choices.Length; i++)
        {
            VisualStateManager.GoToState(choices[i], score == i + 1 ? "Selected" : "Unselected");
            SemanticProperties.SetDescription(choices[i], $"{i + 1} de 5" + (score == i + 1 ? ", selecionado" : ""));
        }
        SelectedScore.Text = $"{score} de 5";
        SaveButton.IsEnabled = true;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (saving || score is < 1 or > 5) return;
        saving = true;
        SaveButton.IsEnabled = false;
        try
        {
            var record = await checkIns.SaveTodayAsync(score);
            await Shell.Current.GoToAsync($"../feedback?id={record.Id}");
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        { ErrorMessage.Text = "Não foi possível salvar. Os registros anteriores foram preservados. Tente novamente."; }
        finally { saving = false; SaveButton.IsEnabled = score > 0; }
    }
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
