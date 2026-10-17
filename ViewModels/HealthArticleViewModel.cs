namespace SukaliCareApp.ViewModels;

[QueryProperty(nameof(Article), "Article")]
public partial class HealthArticleViewModel : BaseViewModel
{
    [ObservableProperty]
    HealthArticle article;

    [RelayCommand]
    public async Task ShareAsync()
    {
        if (Article is null)
        {
            return;
        }

        var text = new StringBuilder();
        text.AppendLine(Article.Title);
        text.AppendLine();
        foreach (string point in Article.KeyPoints)
        {
            text.AppendLine($"• {point}");
        }
        text.AppendLine();
        text.AppendLine("From the Sukali Care app.");

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = Article.Title,
            Text = text.ToString()
        });
    }
}
