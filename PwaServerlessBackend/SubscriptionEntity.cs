using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwaServerlessBackend
{
    public class SubscriptionEntity : TableEntity
    {
        public string Endpoint { get; set; }
        public string p256dh { get; set; }
        public string auth { get; set; }

        public SubscriptionEntity()
        {
            this.RowKey = (System.DateTime.MaxValue.Ticks - System.DateTime.UtcNow.Ticks).ToString("d19");
            this.PartitionKey = "NotificationSubscriptions";
        }
    }
}
