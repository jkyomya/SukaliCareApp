
using CommunityToolkit.Maui.Alerts;
using System.Text.Json;

namespace SukaliCareApp.ViewModels
{
    public partial class CompleteAccountViewModel(HttpProxyService httpProxyService) : BaseViewModel
    {
        [ObservableProperty]
        private string firstname;
        [ObservableProperty]
        private string lastname;
        [ObservableProperty]
        private string phone;
        [ObservableProperty]
        private DateTime dob;
        private readonly HttpProxyService httpProxyService = httpProxyService;

        [RelayCommand]
        public async Task SubmitDetails()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                string email = Preferences.Get(Stores.firebaseUserAccountStore, null);
                AppUser user = new()
                {
                    Email = email,
                    FirstName = Firstname,
                    MiddleName = string.Empty,
                    LastName = Lastname,
                    DoB = Dob,
                    Phone = Phone,
                    ProfilePhoto = string.Empty,
                };
                Preferences.Set(Stores.userDetailsStore, JsonSerializer.Serialize(user));
                Preferences.Set(Stores.completedAccountStore, true);
                await Task.Delay(500);
                //await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                if (await httpProxyService.PostUserDetails(user))
                {
                    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                    await Shell.Current.DisplayAlert("Failed", "Something went wrong. Please try again", "Ok");
                }
            }

        }

        [RelayCommand]
        public async Task GoToDashboard()
        {
            await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
        }

        public async Task RemoteCheckUser()
        {
            IsBusy = true;
            string email = Preferences.Get(Stores.firebaseUserAccountStore, null);
            var proxyResult = await httpProxyService.GetUserDetails(email);
            if(proxyResult == "success")
            {
                await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
            }
            IsBusy = false;
        }
        private bool CheckConnectivity() => Connectivity.NetworkAccess != NetworkAccess.Internet;
    }
}
