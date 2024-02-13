using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.Models
{
    public static class Stores
    {
        public const string preferenceStore = "UserPreferences";
        public const string userDetailsStore = "UserDetails";
        public const string firebaseUserAccountStore = "firebaseAccount";
        public const string completedAccountStore = "completedAccount";
        public const string isLoggedInStore = "isUserLoggedIn";
        public const string cgmDeviceStore = "cgmDevices";
        public const string cgmDeviceReadingsStore = "cgmDeviceReadings";
        public const string connectedDevice = "connectedDevice";
    }
}
