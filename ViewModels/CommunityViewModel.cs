namespace SukaliCareApp.ViewModels;

public partial class CommunityViewModel(CommunityService communityService) : BaseViewModel
{
    private readonly CommunityService community = communityService;

    [ObservableProperty]
    ObservableCollection<SupportGroup> groups = [];

    [ObservableProperty]
    string statusMessage = string.Empty;

    [ObservableProperty]
    bool hasStatus;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        SetStatus(string.Empty);
        try
        {
            IReadOnlyList<SupportGroup> result = await community.GetGroupsAsync();
            Groups.Clear();
            foreach (SupportGroup group in result)
            {
                Groups.Add(group);
            }
            if (Groups.Count == 0)
            {
                SetStatus("No support groups have been added yet. Please check again soon.");
            }
        }
        catch (CommunityException ex)
        {
            SetStatus(ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            SetStatus("Could not load support groups. Pull down to try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenGroupAsync(SupportGroup group)
    {
        if (group is null) return;
        await Shell.Current.GoToAsync(nameof(GroupDetailView), new Dictionary<string, object>
        {
            { "Group", group }
        });
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        HasStatus = !string.IsNullOrEmpty(message);
    }
}
