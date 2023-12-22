

namespace SukaliCareApp.ViewModels
{
    public partial class IntroViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool isLoggedOut = true;

        public async Task OnAppearing()
        {
            UserPreferences pref = new UserPreferences()
            {
                SyncFrequency = 10,
                AllowAutoConnect = true,
                AllowNotifications = true,
                Units = "mmol/l"
            };
            Preferences.Set(Stores.preferenceStore, JsonConvert.SerializeObject(pref));

            //IsBusy = true;
            //if (Preferences.ContainsKey(Stores.userDetailsStore))
            //{
            //    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
            //}
            //else if (Preferences.ContainsKey(Stores.firebaseUserAccountStore))
            //{
            //    await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
            //}
            //else
            //{
            //    IsLoggedOut = true;
            //}

            //IsBusy = false;
        }

        [RelayCommand]
        public async Task Signin()
        {
            IsBusy = true;
            await Task.Delay(200);
            await Shell.Current.GoToAsync($"//{nameof(LoginView)}");
            IsBusy = false;
        }

        [RelayCommand]
        public async Task CreateAccount()
        {
            IsBusy = true;
            await Task.Delay(200);
            await Shell.Current.GoToAsync($"{nameof(SignupView)}");
            IsBusy = false;
        }
    }
}
