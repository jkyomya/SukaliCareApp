
using CommunityToolkit.Maui.Alerts;

namespace SukaliCareApp.ViewModels
{
    public partial class CompleteAccountViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string firstname;
        [ObservableProperty]
        private string lastname;
        [ObservableProperty]
        private string phone;
        [ObservableProperty]
        private DateTime dob;

        [RelayCommand]
        public async Task SubmitDetails()
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
            Preferences.Set(Stores.userDetailsStore, JsonConvert.SerializeObject(user));
            Preferences.Set(Stores.completedAccountStore, true);
            await Task.Delay(500);
            await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
            //if (await httpProxy.PostUserDetails(user))
            //{
            //    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
            //}
            //else
            //{
            //    await Shell.Current.DisplayAlert("Failed", "Something went wrong. Please try again", "Ok");
            //}

        }
        [RelayCommand]
        public async Task GoToDashboard()
        {
            await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
        }
        private bool CheckConnectivity() => Connectivity.NetworkAccess != NetworkAccess.Internet;
    }
}
