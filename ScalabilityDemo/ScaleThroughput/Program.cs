using System;
using System.Threading.Tasks;

namespace CosmosDemo
{
    class Program
    {
 
        static async Task Main(string[] args)
        {
            ScaleThroughput scaleThroughput = new ScaleThroughput();
            GlobalDistribution globalDistribution = new GlobalDistribution();

            bool exit = false;

            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Cosmos DB Demos");
                Console.WriteLine($"-----------------------------------------------------------");
                Console.WriteLine($"[1]   Scale Throughput");
                Console.WriteLine($"[2]   Global Distribution");
                Console.WriteLine($"[3]   Initialize");
                Console.WriteLine($"[4]   Clean up");
                Console.WriteLine($"[5]   Exit");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == '1')
                {
                    Console.Clear();
                    Console.WriteLine("Enter throughput (10,000, 50,000, 100,000");
                    int throughput = Convert.ToInt32(Console.ReadLine());
                    await scaleThroughput.ExecuteAsync(throughput);
                }
                else if (result.KeyChar == '2')
                {
                    Console.Clear();
                    await globalDistribution.ExecuteAsync();
                }
                else if (result.KeyChar == '3')
                {
                    Console.Clear();
                    await scaleThroughput.Initialize();
                }
                else if (result.KeyChar == '4')
                {
                    Console.Clear();
                    await scaleThroughput.CleanUp();
                }
                else if (result.KeyChar == '5')
                {
                    exit = true;
                }
            } 
        }
    }
}
