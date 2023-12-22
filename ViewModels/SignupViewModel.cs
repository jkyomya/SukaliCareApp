
using CommunityToolkit.Maui.Alerts;

namespace SukaliCareApp.ViewModels
{
    public partial class SignupViewModel(FirebaseService firebaseService) : BaseViewModel
    {
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private string confirmPassword;


        [RelayCommand]
        public async Task BackToLoginAsync()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginView)}");
        }
        [RelayCommand]
        public async Task CreateAccountAsync()
        {
            IsBusy = true;
            if (Password == ConfirmPassword)
            {
                if (await firebaseService.CreateFirebaseAccount(Email, Password))
                {
                    Preferences.Set(Stores.firebaseUserAccountStore, Email);
                    await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
                }
                Preferences.Set(Stores.firebaseUserAccountStore, Email);
            }
            else
            {
                await Shell.Current.DisplayAlert("Not matching", "Passwords do not match, please check and try again", "OK");
            }
            IsBusy = false;
        }
        [RelayCommand]
        public async Task GoToDashboard()
        {
            await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
        }

        [RelayCommand]
        public async Task GoToComplete()
        {
            await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
        }
        private bool CheckConnectivity() => Connectivity.NetworkAccess != NetworkAccess.Internet;
    }
}
