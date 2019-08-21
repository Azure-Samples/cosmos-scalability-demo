<img src="https://raw.githubusercontent.com/dennyglee/azure-cosmosdb-spark/master/docs/images/azure-cosmos-db-icon.png" width="75">  &nbsp; Azure Cosmos DB Scalability Demo
==========================================

This Azure Cosmos DB Scalability Demo has been crafted showcasing Cosmos DB's ability to ingest records at scale whilst still providing the flexibility to be scaled back down when not required along the ability to reduce application latency by replicating data to a region closest to the application and thereby increasing application responsiveness.

The first part demonstrates the functionality provided by The Azure Cosmos DB BulkExecutor library for .NET which provides developers out-of-the-box functionality to perform bulk operations in [Azure Cosmos DB](http://cosmosdb.com) and scale the collections allocated RU/s up and down as needed.

The second part demonstrates the ability query data from an Azure Cosmos DB collection geographically   co-located with the application through the use of geo-replication, thereby reducing the round trip latency of the query and in turn increasing application responsiveness. 

<details>
<summary><strong><em>Table of Contents</em></strong></summary>

- [Consuming the Microsoft Azure Cosmos DB BulkExecutor .NET library](#nuget)
- [Setting up your environment](#Azure-Setup)
    - [Deploying your Azure Cosmos DB Account, Database and Collection](#Azure-CosmosDB-Setup)
    - [Deploying your VMs](#Azure-VM-Setup)
    - [Troubleshooting Connectivity](#Azure-NSG-Setup)
    - [Deploying the application](#Azure-App-Deployment)
    - [Scaling your your collection up for Demo](#Azure-Scale)
- [Performance tips](#additional-pointers)
- [Contributing & Feedback](#contributing--feedback)
- [Other relevant projects](#relevant-projects)

</details>

## Consuming the Microsoft Azure Cosmos DB BulkExecutor .NET library

This project includes samples, documentation and performance tips for consuming the BulkExecutor library. You can download the official public NuGet package from [here](https://www.nuget.org/packages/Microsoft.Azure.CosmosDB.BulkExecutor/).

## Setting up your Azure environment

In order to deliver this demonstration you will need an Azure Subscription within wich you will be deploying the requisit Azure Service resources, specificay an Azure Cosmos DB Account, Database and Collection and 2 Azure Visual Studio 2019 VMs

## The original sample application 
You can find the complete sample application program consuming the bulk import API [here](https://github.com/Azure/azure-cosmosdb-bulkexecutor-dotnet-getting-started/blob/master/BulkImportSample/BulkImportSample/Program.cs) - which generates random documents to be then bulk imported into an Azure Cosmos DB collection. You can configure the application settings in *appSettings* [here](https://github.com/Azure/azure-cosmosdb-bulkexecutor-dotnet-getting-started/blob/master/BulkImportSample/BulkImportSample/App.config). 

Additionaly the associated documentation will provide you with additional insight into the inner workings of the demostation code provided here.

## Microsoft.Azure.CosmosDB.BulkExecutor Nnget package 
You can download the Microsoft.Azure.CosmosDB.BulkExecutor nuget package from [here](https://www.nuget.org/packages/Microsoft.Azure.CosmosDB.BulkExecutor/).

------------------------------------------
## Contributing & feedback

This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information
see the [Code of Conduct
FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact
[opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional
questions or comments.

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.

To give feedback and/or report an issue, open a [GitHub
Issue](https://help.github.com/articles/creating-an-issue/).

------------------------------------------

## Other relevant projects

* [Cosmos DB BulkExecutor library for Java](https://github.com/Azure/azure-cosmosdb-bulkexecutor-java-getting-started)