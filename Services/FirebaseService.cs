using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.ApplicationModel.Communication;
using Newtonsoft.Json;
using FirebaseAuthProvider = Firebase.Auth.Providers.FirebaseAuthProvider;

namespace SukaliCareApp.Services
{
    public class FirebaseService
    {
        readonly FirebaseAuthClient authClient;
        // Project settings live in Models/FirebaseConfig.cs.
        private readonly string apiKey = FirebaseConfig.ApiKey;
        private readonly string domain = FirebaseConfig.AuthDomain;
        public FirebaseService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = domain,
                Providers =
                [
                    new GoogleProvider().AddScopes("email"),
                    new EmailProvider()
                ],
                UserRepository = new FileUserRepository("SukaliCareUsers")
            };
            authClient = new FirebaseAuthClient(config);
        }
        public async Task<bool> SigninWithEmail(string email, string password)
        {
            try
            {
                var credentials = EmailProvider.GetCredential(email, password);
                var result = await authClient.SignInWithCredentialAsync(credentials);
                if(!string.IsNullOrEmpty(await result.User.GetIdTokenAsync()))
                {
                    Preferences.Set("firebaseUser", JsonConvert.SerializeObject(result.User.Info));
                    return true;
                }

                return false;
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> SigninWithGoogle()
        {
            try
            {
                var credentials = GoogleProvider.GetCredential("sukalicare");
                var result = await authClient.SignInWithCredentialAsync(credentials);
                if (!string.IsNullOrEmpty(result.User.Uid))
                {
                    Preferences.Set("user", JsonConvert.SerializeObject(result.User.Info));
                    return true;
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> CreateFirebaseAccount(string email, string password)
        {
            try{
                var result = await authClient.CreateUserWithEmailAndPasswordAsync(email, password);
                if(!string.IsNullOrEmpty(await result.User.GetIdTokenAsync()))
                {
                    return await SigninWithEmail(email, password);
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    
        /// <summary>The signed-in user's Firebase ID, or null if nobody is signed in.</summary>
        public string CurrentUserId => authClient.User?.Uid;

        /// <summary>
        /// A fresh ID token for calling Firebase services (such as the community database),
        /// or null if nobody is signed in.
        /// </summary>
        public async Task<string> GetIdTokenAsync()
        {
            try
            {
                if (authClient.User is null)
                {
                    return null;
                }
                return await authClient.User.GetIdTokenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
