using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.Models
{
    public class UserPreferences
    {
        public string Units { get; set; }
        public bool AllowNotifications { get; set; }
        public int SyncFrequency { get; set; }
        public bool AllowAutoConnect { get; set; }
    }
}
