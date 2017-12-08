using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaServerlessBackend
{
    public class Notification
    {
        public NotificationContent notification { get; set; }
    }

    public class NotificationContent
    {
        public string title { get; set; }
        public string body { get; set; }
        public string dir { get; set; }
        public string icon { get; set; }
        public string badge { get; set; }
        public string lang { get; set; }
        public bool renotify { get; set; }
        public bool requireInteraction { get; set; }
        public string tag { get; set; }
        public int[] vibrate { get; set; }
        public NotificationAction[] actions { get; set; }
        public NotificationData data { get; set; }
    }

    public class NotificationAction
    {
        public string action { get; set; }
        public string title { get; set; }
    }

    public class NotificationData
    {
        public int id_fact { get; set; }
        public string fact { get; set; }
    }
}
