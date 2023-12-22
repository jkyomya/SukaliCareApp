using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.Services
{
    public class AppNotificationService
    {
        public AppNotificationService()
        {
                
        }
        public void PublishNotification(string title, string subtitle, string description,
            DateTime dateTime, NotificationRepeat repeatProfile, TimeSpan interval)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(10,10000),
                Title = title,
                Subtitle = subtitle,
                Description = description,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dateTime,
                    RepeatType = repeatProfile,
                    NotifyRepeatInterval = interval
                },
                
            };

            LocalNotificationCenter.Current.Show(request);
        }
    }
}
