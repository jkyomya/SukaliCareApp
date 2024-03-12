namespace SukaliCareApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy = false;

    public AppUser UserDetails
    {
        get => RetrieveUser();
    }

    private static AppUser RetrieveUser()
    {
        string savedUser = Preferences.Get(Stores.userDetailsStore, null);
        if (!string.IsNullOrEmpty(savedUser))
        {
            return JsonConvert.DeserializeObject<AppUser>(savedUser);
        }

        return null;
    }

}
