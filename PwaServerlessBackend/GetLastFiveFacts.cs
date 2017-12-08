using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Text;

namespace PwaServerlessBackend.Retrieve
{
    public static class GetLastFiveFacts
    {
        [FunctionName("GetLastFiveFacts")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("atChuckNorris"));
            CloudTableClient ctClient = storageAccount.CreateCloudTableClient();
            CloudTable cTable = ctClient.GetTableReference("ChuckNorris");
            var query = new TableQuery<ChuckNorrisFactEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "ChuckNorris")).Take(5);
            var results = cTable.ExecuteQuery(query);
            var json = JsonConvert.SerializeObject(results, Formatting.Indented);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
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
