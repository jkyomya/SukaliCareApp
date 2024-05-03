
using System.Text.Json;

namespace SukaliCareApp.ViewModels
{
    public partial class PreferencesViewModel : BaseViewModel
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
                UserDetails = JsonSerializer.Deserialize<AppUser>(details);
            }
            if (!string.IsNullOrEmpty(prefs))
            {
                UserPreferences = JsonSerializer.Deserialize<UserPreferences>(prefs);
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
            if(string.IsNullOrEmpty (result) || result == "Cancel")
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
            var result = await Shell.Current.DisplayPromptAsync("Change sync frequency", "After how many minutes should the phone read values from your smart device",
                "Ok", "Cancel", "minutes", keyboard: Keyboard.Numeric);
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            if (int.TryParse(result, out int duration))
            {
                UserPreferences.SyncFrequency = duration;
                SetDuration = duration;
                SavePreferences();
            }
        }
        public void SavePreferences()
        {
            //Preferences.Remove(Stores.preferenceStore);
            Preferences.Set(Stores.preferenceStore, JsonSerializer.Serialize(UserPreferences));
            //await httpProxy.UpdatePreference(UserPreferences);
        }

        [RelayCommand]
        public static async Task ViewTerms()
        {
            await Browser.OpenAsync("http://sukalicare.nexus-healthcare.org/privacyterms");
        }

        public void NotificationToggled(bool value)
        {
            UserPreferences.AllowNotifications = AllowNotifications;
            SavePreferences();
        }
        public void ConnectionToggled(bool value)
        {
            UserPreferences.AllowAutoConnect = AllowAutoConnect;
            SavePreferences();
        }

        [RelayCommand]
        public static async Task SubmitFeedback()
        {
            await Browser.OpenAsync("http://sukalicare.nexus-healthcare.org/feedback");
        }
        [RelayCommand]
        public static async Task UpdateApp()
        {
            await Shell.Current.DisplayAlert("Update", "No update at the moment", "Ok");
        }
    }
}
