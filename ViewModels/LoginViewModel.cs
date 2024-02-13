


using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace SukaliCareApp.ViewModels
{
    public partial class LoginViewModel(FirebaseService firebaseService) : BaseViewModel
    {
        readonly FirebaseService _firebaseService = firebaseService;
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;

        [RelayCommand]
        public async Task LoginAsync()
        {
                IsBusy = true;
            if (await _firebaseService.SigninWithEmail(Email, Password))
            {
                Preferences.Set(Stores.firebaseUserAccountStore, Email);
                if (Preferences.Get(Stores.completedAccountStore, false))
                {
                    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
                }
                //string remoteRequest = await httpProxy.GetUserDetails(Email);

                //if (remoteRequest == "success")
                //{
                //    Preferences.Set(Stores.firebaseUserAccountStore, Email);
                //    await Shell.Current.GoToAsync($"//{nameof(DashboardView)}");
                //}
                //else if (remoteRequest == "failed")
                //{
                //    await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
                //}
                //else
                //{
                //    return;
                //}

            }
            else
            {
                await Shell.Current.DisplayAlert("Invalid credentials!", "Please check your email or password", "Ok");
            }
                IsBusy = false;
            
        }

        [RelayCommand]
        public async Task CreateAccount()
        {
            await Shell.Current.GoToAsync(nameof(SignupView));
        }

        [RelayCommand]
        public async Task ResetPassword()
        {
            if(!CheckConnectivity())
            {
                Toast.Make("No internet connection");
                return;
                
            }
            await Shell.Current.DisplayAlert("Reset password", "A reset code has been sent to your email", "Proceed to reset", "cancel");
        }
        [RelayCommand]
        public async Task LoginWithGoogle()
        {
            if (CheckConnectivity())
            {
                await Shell.Current.GoToAsync($"//{nameof(CompleteAccountView)}");
            }
            else
            {
                Toast.Make("No internet connection");
            }
            
        }

        private bool CheckConnectivity() => Connectivity.NetworkAccess == NetworkAccess.Internet;
    }
}
