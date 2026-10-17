namespace SukaliCareApp.ViewModels;

[QueryProperty(nameof(Group), "Group")]
public partial class GroupDetailViewModel(CommunityService communityService) : BaseViewModel
{
    private const string GuidelinesAcceptedKey = "communityGuidelinesAccepted";
    private const string DisplayNameKey = "communityDisplayName";

    public const string Guidelines =
        "• Be kind and respectful.\n" +
        "• Share experiences, not medical orders. Always check changes to your treatment with your health worker.\n" +
        "• Do not sell or promote products, herbs or \"cures\".\n" +
        "• Protect privacy. Do not share other people's health details, phone numbers or photos.\n" +
        "• Use a nickname if you prefer.\n" +
        "• In an emergency, go to the nearest health facility. Do not wait for replies here.\n" +
        "Moderators may hide posts that break these rules.";

    private readonly CommunityService community = communityService;

    [ObservableProperty]
    SupportGroup group;

    [ObservableProperty]
    ObservableCollection<CommunityPost> posts = [];

    [ObservableProperty]
    bool isMember;

    [ObservableProperty]
    bool isNotMember = true;

    [ObservableProperty]
    bool isModerator;

    [ObservableProperty]
    string newPostText = string.Empty;

    [ObservableProperty]
    string statusMessage = string.Empty;

    [ObservableProperty]
    bool hasStatus;

    public string GuidelinesText => Guidelines;

    partial void OnGroupChanged(SupportGroup value)
    {
        _ = LoadAsync();
    }

    partial void OnIsMemberChanged(bool value)
    {
        IsNotMember = !value;
    }

    public async Task LoadAsync()
    {
        if (Group is null) return;
        IsBusy = true;
        SetStatus(string.Empty);
        try
        {
            IsModerator = await community.IsModeratorAsync();
            IsMember = await community.IsMemberAsync(Group.Id);
            if (IsMember || IsModerator)
            {
                await LoadPostsAsync();
            }
        }
        catch (CommunityException ex)
        {
            SetStatus(ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            SetStatus("Could not load this group. Pull down to try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RefreshAsync() => await LoadAsync();

    private async Task LoadPostsAsync()
    {
        IReadOnlyList<CommunityPost> result = await community.GetPostsAsync(Group.Id, includeHidden: IsModerator);
        Posts.Clear();
        foreach (CommunityPost post in result)
        {
            Posts.Add(post);
        }
        if (Posts.Count == 0)
        {
            SetStatus("No messages yet. Say hello to the group.");
        }
    }

    [RelayCommand]
    public async Task JoinAsync()
    {
        if (Group is null) return;

        if (!Preferences.Get(GuidelinesAcceptedKey, false))
        {
            bool accepted = await Shell.Current.DisplayAlert(
                "Community guidelines", Guidelines, "I agree", "Cancel");
            if (!accepted) return;
            Preferences.Set(GuidelinesAcceptedKey, true);
        }

        string suggested = Preferences.Get(DisplayNameKey, UserDetails?.FirstName ?? string.Empty);
        string name = await Shell.Current.DisplayPromptAsync(
            "Your name in groups",
            "This is the name other members will see. You can use a nickname.",
            "Join", "Cancel", "e.g. Mama Grace", 40, Keyboard.Text, suggested);
        if (string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim();
        Preferences.Set(DisplayNameKey, name);

        IsBusy = true;
        try
        {
            await community.JoinAsync(Group.Id, name);
            IsMember = true;
            await LoadPostsAsync();
        }
        catch (CommunityException ex)
        {
            await Shell.Current.DisplayAlert("Could not join", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LeaveAsync()
    {
        if (Group is null) return;
        bool sure = await Shell.Current.DisplayAlert("Leave group",
            $"Leave {Group.Name}? You can join again later.", "Leave", "Cancel");
        if (!sure) return;
        try
        {
            await community.LeaveAsync(Group.Id);
            IsMember = false;
            Posts.Clear();
        }
        catch (CommunityException ex)
        {
            await Shell.Current.DisplayAlert("Could not leave", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task SendAsync()
    {
        string text = NewPostText?.Trim();
        if (string.IsNullOrEmpty(text) || Group is null) return;

        string name = Preferences.Get(DisplayNameKey, UserDetails?.FirstName ?? "Member");
        IsBusy = true;
        try
        {
            await community.AddPostAsync(Group.Id, name, text);
            NewPostText = string.Empty;
            SetStatus(string.Empty);
            await LoadPostsAsync();
        }
        catch (CommunityException ex)
        {
            await Shell.Current.DisplayAlert("Not sent", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task PostOptionsAsync(CommunityPost post)
    {
        if (post is null) return;

        var options = new List<string>();
        if (post.IsMine) options.Add("Delete my message");
        else options.Add("Report");
        if (IsModerator) options.Add(post.Hidden ? "Show (moderator)" : "Hide (moderator)");

        string choice = await Shell.Current.DisplayActionSheet("Message", "Cancel", null, options.ToArray());
        try
        {
            switch (choice)
            {
                case "Delete my message":
                    await community.DeleteOwnPostAsync(post);
                    Posts.Remove(post);
                    break;
                case "Report":
                    string reason = await Shell.Current.DisplayActionSheet("Why are you reporting this?", "Cancel", null,
                        "Unkind or abusive", "Selling or promoting products", "Unsafe health advice", "Shares private information", "Other");
                    if (string.IsNullOrEmpty(reason) || reason == "Cancel") return;
                    await community.ReportPostAsync(post, reason);
                    await Shell.Current.DisplayAlert("Thank you", "A moderator will review this message.", "OK");
                    break;
                case "Hide (moderator)":
                    await community.SetHiddenAsync(post, true);
                    await LoadPostsAsync();
                    break;
                case "Show (moderator)":
                    await community.SetHiddenAsync(post, false);
                    await LoadPostsAsync();
                    break;
            }
        }
        catch (CommunityException ex)
        {
            await Shell.Current.DisplayAlert("Something went wrong", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task CallContactAsync()
    {
        if (Group is null || !Group.HasContact) return;
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(Group.ContactPhone);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Call", $"Please call {Group.ContactPhone}", "OK");
        }
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        HasStatus = !string.IsNullOrEmpty(message);
    }
}
