
namespace SukaliCareApp.ViewModels
{
    public partial class PreferencesViewModel(HttpProxyService httpProxy) : BaseViewModel
    {
        [ObservableProperty]
        UserPreferences userPreferences;
        [ObservableProperty]
        string units;
        [ObservableProperty]
        int setDuration;
        [ObservableProperty]
        bool allowNotifications;
        [ObservableProperty]
        bool allowAutoConnect;

        [ObservableProperty]
        AppUser userDetails;

        public void OnAppearing()
        {
            string details = Preferences.Get(Stores.userDetailsStore, null);
            string prefs = Preferences.Get(Stores.preferenceStore, null);
            if (!string.IsNullOrEmpty(details))
            {
                UserDetails = JsonConvert.DeserializeObject<AppUser>(details);
            }
            if (!string.IsNullOrEmpty(prefs))
            {
                UserPreferences = JsonConvert.DeserializeObject<UserPreferences>(prefs);
                Units = UserPreferences.Units;
                SetDuration = UserPreferences.SyncFrequency;
                AllowAutoConnect = UserPreferences.AllowAutoConnect;
                AllowNotifications = UserPreferences.AllowNotifications;
            }
        }

        [RelayCommand]
        public async Task ChangeUnits()
        {
            var result = await Shell.Current.DisplayActionSheet("Change your glucose units", "Cancel", null, ["mmol/l", "mg/dl"]);
            if(string.IsNullOrEmpty (result))
            {
                return;
            }
            UserPreferences.Units = result;
            Units = result;
            SavePreferences();
        }
        [RelayCommand]
        public async Task ChangeFrequency()
        {
            var result = await Shell.Current.DisplayPromptAsync("Change sync frequency", "After how many minutes should teh phone read values from your smart watch",
                "Ok", "Cancel", "minutes", keyboard: Keyboard.Numeric);
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            int.TryParse(result, out int duration);
            UserPreferences.SyncFrequency = duration;
            SetDuration = duration;
            SavePreferences();
        }
        public async void SavePreferences()
        {
            Preferences.Remove(Stores.preferenceStore);
            Preferences.Set(Stores.preferenceStore, JsonConvert.SerializeObject(UserPreferences));
            //await httpProxy.UpdatePreference(UserPreferences);
        }

        [RelayCommand]
        public async Task ViewTerms()
        {
            await Browser.OpenAsync("http://sukalicare.nexus-healthcare.org/privacyterms");
        }

        [RelayCommand]
        public async Task SubmitFeedback()
        {
            await Browser.OpenAsync("http://sukalicare.nexus-healthcare.org/feedback");
        }
        [RelayCommand]
        public async Task UpdateApp()
        {
            await Shell.Current.DisplayAlert("Update", "No update at the momment", "Ok");
        }
    }
}
