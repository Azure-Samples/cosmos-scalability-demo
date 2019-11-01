using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDemo
{
    public class Utils
    {
        public static async Task<Container> GetContainerOrCreate(CosmosClient client, string databaseId, string containerId, string partitionKeyPath, int throughput)
        {
            Container container;
            try
            {
                container = client.GetContainer(databaseId, containerId);
                await container.ReadContainerAsync(); //throws exception if not created
                //check container throughput and scale up/down if needed (Note: container should be manually scaled up/down before running this)
                int? currentThroughput = await container.ReadThroughputAsync();
                if(currentThroughput != throughput)
                {
                    Console.WriteLine($"Scaling container from {currentThroughput} to {throughput}");
                    await container.ReplaceThroughputAsync(throughput);
                }
            }
            catch
            {
                DatabaseResponse dbResponse = await client.CreateDatabaseIfNotExistsAsync(databaseId);
                ContainerResponse cResponse = await dbResponse.Database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath, throughput);
                container = cResponse.Container;
                await VerifyContainerReplicated(container); //Verify container has replicated
            }

            return container;
        }
        private static async Task<bool> VerifyContainerReplicated(Container container)
        {
            bool isReplicated = false;
            bool notifyOnce = false;

            while (!isReplicated)
            {
                try
                {
                    await container.ReadContainerAsync();
                    isReplicated = true; //hit this line then container has replicated to other regions.
                    if (notifyOnce)
                        Console.WriteLine("Resource is replicated and available");
                }
                catch
                {
                    if (!notifyOnce)
                    {
                        Console.WriteLine("Waiting for container to replicate in all regions");
                        notifyOnce = true;
                    }
                    //swallow any errors and wait 250ms to retry
                    await Task.Delay(250);
                }
            }
            return isReplicated;
        }
        public static async Task CleanUp(CosmosClient client, string databaseId)
        {
            try
            {
                await client.GetDatabase(databaseId).DeleteAsync();
            }
            catch { }
        }
    }
}
