using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using RestSharp;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using WebPush;

namespace PwaServerlessBackend.Insert
{
    public static class AddFact
    {

        [FunctionName("AddFact")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            //log.Info("C# HTTP trigger function processed a request.");

            // --------- Récupération d'une blague à la con
            string chuckEndpoint = @"https://api.icndb.com"; //jokes/random
            RestClient client = new RestClient(chuckEndpoint);
            RestRequest request = new RestRequest("/jokes/random/");
            var response = client.Get(request);
            var responseContent = JsonConvert.DeserializeObject<ChuckNorrisFactResponse>(response.Content);


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("atChuckNorris"));

            // -------- Ajout de la blague dans une Azure Table
            CloudTableClient ctClient = storageAccount.CreateCloudTableClient();
            CloudTable cTable = ctClient.GetTableReference("ChuckNorris");
            ChuckNorrisFactEntity cnEntity = new ChuckNorrisFactEntity()
            {
                Id = responseContent.Value.Id,
                Joke = responseContent.Value.Joke
            };
            TableOperation insertOperation = TableOperation.Insert(cnEntity);
            cTable.Execute(insertOperation);

            // --- Notify everyone
            CloudTableClient usersClient = storageAccount.CreateCloudTableClient();
            CloudTable usersTable = usersClient.GetTableReference("NotificationSubscriptions");
            var query = new TableQuery<SubscriptionEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "NotificationSubscriptions"));
            var users = usersTable.ExecuteQuery(query);

            Guid notificationTag = Guid.NewGuid();
            foreach (SubscriptionEntity user in users)
            {
                NotifyEveryone(user.Endpoint, user.p256dh, user.auth, responseContent, notificationTag, log);
            }

            return req.CreateResponse(HttpStatusCode.OK, responseContent.Value.Joke);
        }

        public static bool NotifyEveryone(string endpoint, string p256dh, string auth, ChuckNorrisFactResponse content, Guid tagNotif, TraceWriter log)
        {
            // Send Notification Confirmation
            var subject = System.Environment.GetEnvironmentVariable("subject");
            var publicKey = System.Environment.GetEnvironmentVariable("publicKey");
            var privateKey = System.Environment.GetEnvironmentVariable("privateKey");

            var subscription = new PushSubscription(endpoint, p256dh, auth);
            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

            var webPushClient = new WebPushClient();

            var notif = new Notification()
            {
                notification = new NotificationContent()
                {
                    title = $"Fact {content.Value.Id} has arrived !",
                    body = content.Value.Joke,
                    dir = "auto",
                    icon = "https://digitalmacgyver.files.wordpress.com/2012/04/chucknorris_approved.jpg",
                    badge = "https://digitalmacgyver.files.wordpress.com/2012/04/chucknorris_approved.jpg",
                    vibrate = new int[]
                    {
                        300,100,400
                    },
                    lang = "en",
                    renotify = true,
                    tag = tagNotif.ToString(),
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
                        id_fact = int.Parse(content.Value.Id),
                        fact = content.Value.Joke
                    }
                }
            };

            var json = JsonConvert.SerializeObject(notif, Formatting.Indented);

            try
            {
                webPushClient.SendNotification(subscription, json, vapidDetails);
                return true;
            }
            catch (WebPushException exception)
            {
                log.Info(exception.Message);
                return false;
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
                return false;
            }

        }
    }

    public class ChuckNorrisFactResponse
    {
        public string Type { get; set; }
        public ChuckNorrisFact Value { get; set; }
    }

    public class ChuckNorrisFact
    {
        public string Id { get; set; }
        public string Joke { get; set; }
        public string[] Categories { get; set; }
    }

    public class ChuckNorrisFactEntity : TableEntity
    {
        public string Id { get; set; }
        public string Joke { get; set; }

        public ChuckNorrisFactEntity()
        {
            this.RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19");
            this.PartitionKey = "ChuckNorris";
        }
    }
}
