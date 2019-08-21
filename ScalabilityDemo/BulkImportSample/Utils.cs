//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace BulkImportSample
{
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    class Utils
    {
        /// <summary>
        /// Get the collection if it exists, null if it doesn't.
        /// </summary>
        /// <returns>The requested collection.</returns>
        static internal DocumentCollection GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            if (GetDatabaseIfExists(client, databaseName) == null)
            {
                return null;
            }

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
                .Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Get the database if it exists, null if it doesn't.
        /// </summary>
        /// <returns>The requested database.</returns>
        static internal Database GetDatabaseIfExists(DocumentClient client, string databaseName)
        {
            return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Create a partitioned collection.
        /// </summary>
        /// <returns>The created collection.</returns>
        static internal async Task<DocumentCollection> CreatePartitionedCollectionAsync(DocumentClient client, string databaseName,
            string collectionName, int collectionThroughput)
        {
            PartitionKeyDefinition partitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { ConfigurationManager.AppSettings["CollectionPartitionKey"] }
            };
            DocumentCollection collection = new DocumentCollection { Id = collectionName, PartitionKey = partitionKey };

            try
            {
                collection = await client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    collection,
                    new RequestOptions { OfferThroughput = collectionThroughput });
            }
            catch (Exception e)
            {
                throw e;
            }

            return collection;
        }

        static internal String GenerateRandomDocumentString(String id, String partitionKeyProperty, String parititonKeyValue)
        {
            var eventTypes = new string[] { "Harsh_break", "Airbag_deploy", "Check_engine_light" };
            string eventName = eventTypes[int.Parse(parititonKeyValue) % 3];
            return "{\n" +
                "    \"id\": \"" + id + "\",\n" +
                "    \"" + partitionKeyProperty + "\": \"" + parititonKeyValue + "\",\n" +
                "    \"EventName\": \"" + eventName + "\",\n" +
                "    \"Description\": \"\",\n" +
                "    \"s1\": \"38442291.3\",\n" +
                "    \"s2\": \"23959381.2\",\n" +
                "    \"s3\": \"148\",\n" +
                "    \"s4\": \"323\",\n" +
                "    \"s5\": \"32395.9\",\n" +
                "    \"s6\": \"8732\"" +
                "}";
        }
    }
}