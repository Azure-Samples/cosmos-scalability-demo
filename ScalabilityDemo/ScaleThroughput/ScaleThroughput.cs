using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosDemo
{
    class ScaleThroughput
    {
        static string endpoint;
        static string key;
        static string databaseId;
        static string containerId;
        static string partitionKeyPath;
        static int throughput;
        static int itemsToCreate;
        static int workers;
        static int itemsPerWorker;
        static CosmosClient client;

        static ConcurrentDictionary<int, string> results;
        bool complete;

        public ScaleThroughput()
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
            itemsToCreate = int.Parse(configuration["itemsToCreate"]);
            workers = int.Parse(configuration["Workers"]);
            itemsPerWorker = itemsToCreate / workers;

            try
            {
                client = new CosmosClient(endpoint, key, new CosmosClientOptions
                {
                    ApplicationRegion = "West US 2",
                    ConnectionMode = ConnectionMode.Direct,
                    AllowBulkExecution = true
                });
            }
            catch (CosmosException e)
            {
                Console.WriteLine("Caught AggregateException in Main, Inner Exception:\n" + e);
                Console.ReadKey();
            }
        }
        public async Task ExecuteAsync(int throughput)
        {
            Container container = null;
            try
            {
                //Get container
                container = await Utils.GetContainerOrCreate(client, databaseId, containerId, partitionKeyPath, throughput);

                Console.WriteLine("Summary:");
                Console.WriteLine("--------------------------------------------------------------------- ");
                Console.WriteLine($"Endpoint: {endpoint}");
                Console.WriteLine($"Container: {databaseId}.{containerId}");
                Console.WriteLine($"Initiating Bulk Execution of {itemsToCreate} items with {workers} workers at {throughput} RU/s.");
                Console.WriteLine("---------------------------------------------------------------------\n");

                //write results to thread-safe collection
                results = new ConcurrentDictionary<int, string>();

                //Task to output worker progress from concurrent dictionary every second
                await Task.Factory.StartNew(() => OutputWorkerStats());

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                List<Task> tasks = new List<Task>(workers);
                for (int workerId = 0; workerId < workers; workerId++)
                {
                    results.TryAdd(workerId, string.Empty); //Add new worker item
                    tasks.Add(CreateItemsAsync(container, workerId, itemsPerWorker));
                }

                await Task.WhenAll(tasks);
                complete = true; //Turn off OutputWorkerStats()
                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;
                OutputFinalStats(elapsed);
                Console.WriteLine("\nPress any key to continue.");
                Console.ReadKey();
            }
            catch (AggregateException ae)
            {
                Console.Write("Aggregate Exception:");
                foreach (Exception e in ae.InnerExceptions)
                {
                    Console.WriteLine($"Inner Exception: {e.Message}");
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
        private async Task CreateItemsAsync(Container container, int workerId, int itemstoInsert)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int itemsCreated = 0;
            int ruConsumed = 0;
            long elapsedMilliseconds = 0;
            long elapsedSeconds = 0;

            List<Task> tasks = new List<Task>(Convert.ToInt32(itemstoInsert));
            List<VehicleEvent> events = VehicleEvent.CreateEvent(itemstoInsert);

            foreach (VehicleEvent e in events)
            {
                tasks.Add(
                    container.CreateItemAsync<VehicleEvent>(e, new PartitionKey(e.vin))
                    .ContinueWith((Task<ItemResponse<VehicleEvent>> task) =>
                    {
                        if (!task.IsCompletedSuccessfully)
                        {
                            AggregateException innerExceptions = task.Exception.Flatten();
                            CosmosException cosmosException = innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) as CosmosException;
                            Console.WriteLine($"Item {e.id} failed with status code {cosmosException.StatusCode}");
                        }
                        else
                        {
                            itemsCreated += 1;
                            ruConsumed += Convert.ToInt32(task.Result.RequestCharge); 
                            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                            elapsedSeconds = (elapsedMilliseconds < 1000) ? 1 : (elapsedMilliseconds / 1000);
                            string status = $"Worker {workerId} Inserted {itemsCreated} docs @ {(itemsCreated / elapsedSeconds)} writes/s, {ruConsumed / elapsedSeconds} RU/s in {elapsedSeconds} sec";
                            results.AddOrUpdate(workerId, status, (workerId, x) => { return status; });
                        }
                    }));
            }
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
        }
        private async Task OutputWorkerStats()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                while (!complete)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Console.Clear();

                    Console.WriteLine("Concurrent Workers Summary");
                    Console.WriteLine("--------------------------------------------------------------------- ");

                    foreach (var item in results)
                    {
                        string r = item.Value;
                        Console.WriteLine(r);
                    }
                    Console.WriteLine("---------------------------------------------------------------------\n");
                }
            }
            catch (AggregateException ae)
            {
                Console.Write("Aggregate Exception:");
                foreach (Exception e in ae.InnerExceptions)
                {
                    Console.WriteLine($"Inner Exception: {e.Message}");
                }
            }
        }
        private void OutputFinalStats(long elapsed)
        {
            Console.Clear();

            Console.WriteLine("Final Workers Summary");
            Console.WriteLine("--------------------------------------------------------------------- ");

            foreach (var item in results)
            {
                string r = item.Value;
                Console.WriteLine(r);
            }
            Console.WriteLine($"Total Elapsed time: {elapsed / 1000}");
            Console.WriteLine("---------------------------------------------------------------------\n");
        }
        public async Task Initialize()
        {
            await Utils.GetContainerOrCreate(client, databaseId, containerId, partitionKeyPath, throughput);
        }
        public async Task CleanUp()
        {
            await Utils.CleanUp(client, databaseId);
        }
    }
}
