using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System;

namespace CosmosDbCrudFunctions
{
    public static class CrudFunction
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDbEndpointUri");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");
        private static readonly string DatabaseName = "ToDoList";
        private static readonly string ContainerName = "Items";
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        [Function("CosmosDbCrud")]
        public static async Task<HttpResponseData> CosmosDbCrud(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = "items/{id?}")] HttpRequestData req,
            FunctionContext executionContext,
            string id)
        {
            var logger = executionContext.GetLogger("CosmosDbCrud");
            logger.LogInformation($"C# HTTP trigger function processed a {req.Method} request.");

            Container container = cosmosClient.GetContainer(DatabaseName, ContainerName);

            switch (req.Method)
            {
                case "POST":
                    return await CreateItemAsync(req, container);
                case "GET":
                    return await ReadItemAsync(req, container, id);
                case "PUT":
                    return await UpdateItemAsync(req, container, id);
                case "DELETE":
                    return await DeleteItemAsync(req, container, id);
                default:
                    var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await response.WriteStringAsync("Unsupported HTTP method.");
                    return response;
            }
        }

        private static async Task<HttpResponseData> CreateItemAsync(HttpRequestData req, Container container)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            await container.CreateItemAsync(data, new PartitionKey(data.id.ToString()));
            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            await response.WriteStringAsync("Item created successfully");
            return response;
        }

        private static async Task<HttpResponseData> ReadItemAsync(HttpRequestData req, Container container, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Return all items if no ID is provided
                return await GetAllItemsAsync(req, container);
            }
            
            try
            {
                ItemResponse<dynamic> responseItem = await container.ReadItemAsync<dynamic>(id, new PartitionKey(id));
                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                string responsestring = JsonConvert.SerializeObject(responseItem.Resource);
                await response.WriteStringAsync(responsestring);
                return response;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var response = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await response.WriteStringAsync("Item not found");
                return response;
            }
        }

        private static async Task<HttpResponseData> GetAllItemsAsync(HttpRequestData req, Container container)
        {
             var query = new QueryDefinition("SELECT * FROM c");
             FeedIterator<dynamic> resultSetIterator = container.GetItemQueryIterator<dynamic>(query);
        
             List<dynamic> items = new List<dynamic>();
             while (resultSetIterator.HasMoreResults)
             {
                 FeedResponse<dynamic> response = await resultSetIterator.ReadNextAsync();
                 items.AddRange(response);
             }
        
             var responseAll = req.CreateResponse(System.Net.HttpStatusCode.OK);
             await responseAll.WriteStringAsync(JsonConvert.SerializeObject(items));
             return responseAll;
        }

        private static async Task<HttpResponseData> UpdateItemAsync(HttpRequestData req, Container container, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            await container.UpsertItemAsync(data, new PartitionKey(id));
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Item updated successfully");
            return response;
        }

        private static async Task<HttpResponseData> DeleteItemAsync(HttpRequestData req, Container container, string id)
        {
            await container.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Item deleted successfully");
            return response;
        }
    }
}
