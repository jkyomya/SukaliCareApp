

using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;

namespace SukaliCareApp.ViewModels
{
    public partial class IntroViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool isLoggedOut = true;

        public async Task OnAppearing()
        {
            try
            {
                UserPreferences pref = new()
            {
                SyncFrequency = 10,
                AllowAutoConnect = true,
                AllowNotifications = true,
                Units = "mmol/l"
            };
            Preferences.Set(Stores.preferenceStore, JsonConvert.SerializeObject(pref));

            IsBusy = true;
                if (Preferences.ContainsKey(Stores.userDetailsStore))
                {
                    await Task.Delay(1000);

                    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                }
                else if (Preferences.ContainsKey(Stores.firebaseUserAccountStore))
                {
                    await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
                }
                else
                {
                    IsLoggedOut = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "Ok");
            }
            finally
            {
                IsBusy = false;
            }

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
