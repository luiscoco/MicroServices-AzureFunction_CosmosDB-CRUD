# How to integrate all Azure CosmosDB CRUD operations in one Azure Function

See the source code for this example in these repo: 

https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD

## 1. Prerequisite

Create Azure CosmosDB account, database and collection

e navigate to **Azure CosmosDB** service and we click in this option to create a new account:

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/3822db85-173d-411d-8383-7f97d07c05f4)

We click on **Azure CosmosDB account** button

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/c1080788-bfc0-445e-9084-ce0f0772044a)

Now we select the option **Azure Cosmos DB for NoSQL**, and we press the **create** button

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/69182e3f-9493-41a8-ab6b-92308a9bdddf)

In following screen we input the required data for creating the service

We create a new **ResourceGroup name**: myRG

We set the **account name**: mycosmosdbluis1974 

We choose the service **location**: West Europe

Capacity mode: **serverless**

![image](https://github.com/user-attachments/assets/d2a983b4-48e2-4bdf-86ac-dab5ab40a8ab)

We navigate to the **Data Explorer** page and we create a **New Database** and a **New Container**

We first create a **New Database**. We input the **DatabaseId**: ToDoList

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/978b2f67-01c3-4711-95fd-e0eeb75a99c1)

We also create a **New Container**: Items

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/10e1d766-9c53-4ab0-9624-52f257340480)

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD_operations/assets/32194879/a7f40ff9-af30-4d1d-b04a-55fe66d7b4bb)

We insert the new items in the Azure CosmosDB

This is the **new item** json file:

```json
{
  "id": "1",
  "name": "Sample Item",
  "category": "Sample Category"
}
```

We click in **Items** and then **New Item**

Then we copy and paste the json content and press **Save** button

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD_operations/assets/32194879/08809426-85e4-4e75-a938-6d779989b218)

## 2. Create Azure Functions Projects

We install the Project Templates with dotnet

```
dotnet new --install Microsoft.Azure.Functions.Worker.ProjectTemplates
```

First, we create a new Azure Functions project targeting .NET 8

```
dotnet new func -n CosmosDbCrudFunctions --Framework net8.0
cd CosmosDbCrudFunctions
```

We create a new file for the CRUD operations, see the project folders structure

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/e31ff742-060e-4625-aedb-568ce5fa856b)

## 3. Add NuGet Package for Cosmos DB

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/7c56eded-258e-4dac-92ce-ad6448612815)

## 4. Implement the CRUD operations in one Azure Function

```csharp
﻿using Microsoft.Azure.Functions.Worker;
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
```

## 5. Configure Local Settings

In your **local.settings.json** file, add your Cosmos DB connection string as follows

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbEndpointUri": "",
    "CosmosDbPrimaryKey": ""
  }
}
```

We get the CosmosDB URI and the PrimaryKey from the Azure Portal

![image](https://github.com/user-attachments/assets/b432b99f-3210-40b0-bbc9-81db757a25f9)

## 6. Run and test the Azure Function with Postman

We run and test the Create Azure Function and we get this output

We test the function with **Postman** 

We first test the **POST** request

http://localhost:7112/api/items

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/5a47a3c2-8ff5-4983-90f7-3bbd65951256)

We also test the **GET** by id request

http://localhost:7112/api/items/1

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/976cd139-151b-4221-ab77-6b99c17cd885)

We test the **PUT** by id request

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/87dbbf1b-65b4-42b4-ae67-492a564f1ddd)

We finally test the **DELETE** by id 

![image](https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD/assets/32194879/982bc3ab-ceb5-46cf-b78c-6f9778211127)

## 7. How to depooy the Azure Function from Visual Studio 2022 to Azure Portal

Right click on the Azure Function name 

![image](https://github.com/user-attachments/assets/f40847a6-d582-4d68-a583-70ce1b3c6f22)

Select the menu option Publish 

![image](https://github.com/user-attachments/assets/9d5fe7ea-ec17-46e4-8e02-2c67d63b2b53)

Press the button **Add a Publising Profile**

![image](https://github.com/user-attachments/assets/e7178bb3-484b-4dbc-986a-3f4cfebfa867)

Select where to publish the application. We select **Azure**

![image](https://github.com/user-attachments/assets/7e79d010-63e2-486a-b2ba-209b15c0cf4d)

Select the specific publising target **Azure Function (Linux)**

![image](https://github.com/user-attachments/assets/8abbc6db-f67a-45e0-b952-c061884cfe3a)

**IMPORTANT NOTE**: it is mandatory to create in Azure Portal or with Azure CLI and Azure Fuction to host the Azure Function developed in Visual Studio 2022
Otherwise no available service will appear in the publishing list. 

For more details about it see the URL: https://github.com/luiscoco/AzureFunctions_CreateFunctionInVisualStudio2022

Select the Azure Function in Azure Portal and press the **Finish** button

![image](https://github.com/user-attachments/assets/7dc7c371-9f9b-41fa-974a-cfd0ea5a7924)

We get the confirmation messages after publising and press the **Close** button

![image](https://github.com/user-attachments/assets/90fd995b-6816-42ab-80ba-b7a4b23f2b37)

We press the **Publish** button

![image](https://github.com/user-attachments/assets/877c79ac-f979-4247-9030-b9f5597f7ff6)

We verify in the Azure Portal the deployment

![image](https://github.com/user-attachments/assets/e65b7ba1-94d2-4157-a832-a2e679ca26a3)

![image](https://github.com/user-attachments/assets/9b40a306-004e-40d2-a7dd-811acfa824f4)

![image](https://github.com/user-attachments/assets/98b12602-0bc3-4e1d-8e54-54a75aef81be)

![image](https://github.com/user-attachments/assets/6419c7bc-0650-43fe-846d-cc5b734a138a)

![image](https://github.com/user-attachments/assets/68660586-c6ea-460f-b986-8cfc10ec8085)

We also test the Azure Function in Postman

https://mycosmosdbcrudazurefunction.azurewebsites.net/api/items/?code=SWCyUgbqY6yDdAMhmP69EekVukGjwhPl2oQWWWMoUDK-AzFucN7Zjw%3D%3D

![image](https://github.com/user-attachments/assets/d20e8c69-433c-494a-938b-decc2edfc8a2)

https://mycosmosdbcrudazurefunction.azurewebsites.net/api/items/1?code=SWCyUgbqY6yDdAMhmP69EekVukGjwhPl2oQWWWMoUDK-AzFucN7Zjw%3D%3D

![image](https://github.com/user-attachments/assets/6238dcea-9712-4862-9bb5-8b4144db664d)




