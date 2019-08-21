using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace GlobalDemo
{
    class Program
    {
        private static readonly string EndpointUrl = ConfigurationManager.AppSettings["EndPointUrl"];
        private static readonly string AuthorizationKey = ConfigurationManager.AppSettings["AuthorizationKey"];
        private static DocumentClient client;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            var connectionPolicy = new ConnectionPolicy()
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            //Setting read region selection preference
            connectionPolicy.PreferredLocations.Add(LocationNames.SouthCentralUS); // first preference
            connectionPolicy.PreferredLocations.Add(LocationNames.WestUS2); // third preference
            connectionPolicy.PreferredLocations.Add(LocationNames.SoutheastAsia); // second preference

            client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey, connectionPolicy: connectionPolicy);
            client.OpenAsync().ConfigureAwait(false);
            FeedOptions feedOptions = new FeedOptions
            {
                EnableCrossPartitionQuery = true,
                MaxDegreeOfParallelism = 133
            };

            // Get latest record for vin = 99971. 
            //var query = "SELECT TOP 1 * FROM c WHERE c.vin = 99971";
            var query = "SELECT TOP 1 * FROM c WHERE c.newProp = 'red' and c.vin = 99971";

            while (true)
            {
                var sw = new Stopwatch();
                sw.Start();
                
                var document = client.CreateDocumentQuery<dynamic>(UriFactory.CreateDocumentCollectionUri("Demo", "ReadyVehicleData"), query, feedOptions ).ToList().FirstOrDefault();

                sw.Stop();

                Console.WriteLine($"Read document in {sw.ElapsedMilliseconds} ms from {client.ReadEndpoint}");

                Thread.Sleep(1000);
            }
        }
    }
}
