using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using WebPush;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace PwaServerlessBackend
{
    public static class Subscribe
    {
        [FunctionName("Subscribe")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            SubscriptionRequest subscriptionReq = JsonConvert.DeserializeObject<SubscriptionRequest>(req.Content.ReadAsStringAsync().Result);

            // Store notification subscription
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("atChuckNorris"));
            CloudTableClient ctClient = storageAccount.CreateCloudTableClient();
            CloudTable cTable = ctClient.GetTableReference("NotificationSubscriptions");
            SubscriptionEntity subEntity = new SubscriptionEntity()
            {
                Endpoint = subscriptionReq.subscription.endpoint,
                p256dh = subscriptionReq.subscription.keys.p256dh,
                auth = subscriptionReq.subscription.keys.auth
            };
            TableOperation insertOperation = TableOperation.Insert(subEntity);
            cTable.Execute(insertOperation);

            // Send Notification Confirmation
            var subject = System.Environment.GetEnvironmentVariable("subject");
            var publicKey = System.Environment.GetEnvironmentVariable("publicKey");
            var privateKey = System.Environment.GetEnvironmentVariable("privateKey");

            var subscription = new PushSubscription(subscriptionReq.subscription.endpoint, subscriptionReq.subscription.keys.p256dh, subscriptionReq.subscription.keys.auth);
            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

            var webPushClient = new WebPushClient();

            var notif = new Notification()
            {
                notification = new NotificationContent()
                {
                    title = "Subscription completed",
                    body = "You have successfully subscribed to our great notifications from Chuck Norris",
                    dir = "auto",
                    icon = "https://digitalmacgyver.files.wordpress.com/2012/04/chucknorris_approved.jpg",
                    badge = "https://digitalmacgyver.files.wordpress.com/2012/04/chucknorris_approved.jpg",
                    vibrate = new int[]
                    {
                        300,100,400
                    },
                    lang = "en",
                    renotify = true,
                    requireInteraction = false,
                    actions = new NotificationAction[]
                    {
                        new NotificationAction
                        {
                            action="open",
                            title="Open"
                        }
                    },
                    data = new NotificationData
                    {
                        id_fact = 0,
                        fact = "This the confirmation notification"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(notif, Formatting.Indented);

            try
            {
                webPushClient.SendNotification(subscription, json, vapidDetails);
            }
            catch (WebPushException exception)
            {

            }


            return req.CreateResponse(HttpStatusCode.OK, "Subscription completed");
        }
    }


}

