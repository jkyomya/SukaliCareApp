namespace SukaliCareApp.ViewModels;

public partial class HealthInfoViewModel(HealthContentService contentService) : BaseViewModel
{
    private readonly HealthContentService contentService = contentService;

    [ObservableProperty]
    ObservableCollection<HealthTopic> topics = [];

    [ObservableProperty]
    string disclaimer = string.Empty;

    public async Task LoadAsync()
    {
        if (Topics.Count > 0)
        {
            return;
        }

        IsBusy = true;
        HealthContent content = await contentService.GetContentAsync();
        Disclaimer = content.Disclaimer;
        Topics.Clear();
        foreach (HealthTopic topic in content.Topics)
        {
            Topics.Add(topic);
        }
        IsBusy = false;
    }

    [RelayCommand]
    public async Task OpenArticleAsync(HealthArticle article)
    {
        if (article is null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(HealthArticleView), new Dictionary<string, object>
        {
            { "Article", article }
        });
    }

    [RelayCommand]
    public async Task OpenOnlinePostsAsync()
    {
        await Shell.Current.GoToAsync(nameof(LibraryView));
    }
}
