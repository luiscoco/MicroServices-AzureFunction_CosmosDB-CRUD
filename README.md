# How to integrate all Azure CosmosDB CRUD operations in one Azure Function

See the source code for this example in these repo: https://github.com/luiscoco/MicroServices-AzureFunction_CosmosDB-CRUD

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

We choose the service **location**: France Central

Capacity mode: **serverless**

![image](https://github.com/luiscoco/MicroServices_dotNET8_CRUD_WebAPI-AzureCosmosDB/assets/32194879/ed5a1407-0303-44b3-b528-7a7fc1398d80)

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

## 3. Add NuGet Package for Cosmos DB




## 4. Implement each CRUD Operation in one Azure Function


