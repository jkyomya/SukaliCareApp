using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace SukaliCareApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const int PermissionRequestCode = 102;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Ask for everything the app needs in one prompt, using this activity
            // (Platform.CurrentActivity can still be null this early in start-up).
            var needed = new List<string>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                needed.Add(Manifest.Permission.BluetoothConnect);
                needed.Add(Manifest.Permission.BluetoothScan);
            }
            else
            {
                needed.Add(Manifest.Permission.Bluetooth);
                needed.Add(Manifest.Permission.AccessFineLocation);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                needed.Add(Manifest.Permission.PostNotifications);
            }

            string[] missing = needed
                .Where(p => ContextCompat.CheckSelfPermission(this, p) != Permission.Granted)
                .ToArray();

            if (missing.Length > 0)
            {
                ActivityCompat.RequestPermissions(this, missing, PermissionRequestCode);
            }
        }
    }
}
