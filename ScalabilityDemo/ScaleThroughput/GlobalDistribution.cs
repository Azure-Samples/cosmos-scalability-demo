using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosDemo
{
    class GlobalDistribution
    {
        static string endpoint;
        static string key;
        static string databaseId;
        static string containerId;
        static string partitionKeyPath;
        static int throughput;
        static CosmosClient client;

        public GlobalDistribution()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build();

            endpoint = configuration["Endpoint"];
            key = configuration["Key"];
            databaseId = configuration["DatabaseId"];
            containerId = configuration["ContainerId"];
            partitionKeyPath = configuration["PartitionKeyPath"];
            throughput = int.Parse(configuration["Throughput"]);

            client = new CosmosClient(endpoint, key, new CosmosClientOptions
            {
                ApplicationRegion = "West US 2", //Southeast Asia
                ConnectionMode = ConnectionMode.Direct
            });
        }

        public async Task ExecuteAsync()
        {
            Container container = await Utils.GetContainerOrCreate(client, databaseId, containerId, partitionKeyPath, throughput);

            QueryDefinition query;

            string vin = "QW9AP88RXFVJ76341";
            query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vin = @vin")
                .WithParameter("@vin", vin);

            //string newProp = "100";
            //query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vin = @vin and c.newProp = @newProp")
            //    .WithParameter("@vin", vin)
            //    .WithParameter("@newProp", newProp);

            Console.WriteLine($"Read 100 items from {containerId}");
            Console.WriteLine("--------------------------------------------------------------------- ");

            for(int i = 0; i < 100; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                FeedIterator<VehicleEvent> resultSetIterator = container.GetItemQueryIterator<VehicleEvent>(
                    query,
                    requestOptions: new QueryRequestOptions(){ PartitionKey = new PartitionKey(vin) });

                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<VehicleEvent> response = await resultSetIterator.ReadNextAsync();
                }
                stopwatch.Stop();
                Console.WriteLine($"Read item in {stopwatch.ElapsedMilliseconds} ms from {client.ClientOptions.ApplicationRegion}");

            }
            Console.WriteLine("\nPress any key to continue.");
            Console.ReadKey();
        }
    }
}
