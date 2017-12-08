using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaServerlessBackend
{
    public class SubscriptionRequest
    {
        public string action { get; set; }
        public SubscriptionInfos subscription { get; set; }
    }

    public class SubscriptionInfos
    {
        public string endpoint { get; set; }
        public string expirationTime { get; set; }
        public SubscriptionKeys keys { get; set; }
    }

    public class SubscriptionKeys
    {
        public string p256dh { get; set; }
        public string auth { get; set; }
    }
}
