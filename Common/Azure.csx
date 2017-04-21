#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "../Models/Game.csx"
#load "../Common/CreateGame.csx"
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class Azure
{
    private readonly CloudStorageAccount _cloudStorageAccount;
    private readonly TraceWriter _log;

    public Azure(TraceWriter log)
    {
        _log = log;
        var conn = System.Configuration.ConfigurationManager.ConnectionStrings["AzureBlob"].ConnectionString;
        _log.Info($"Connection String = {conn}");
        _cloudStorageAccount = CloudStorageAccount.Parse(conn);
        
    }


    public bool StateExists(string appId, string key)
    {
        // Create the blob client.
        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();

        // Retrieve reference to container. Containers use same name rules as tables (see table name limitations).
        var container = blobClient.GetContainerReference($"states{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if container is already created manually in Azure 
        container.CreateIfNotExists();

        var blockBlob = container.GetBlockBlobReference(key);

        //workaround for Exist()
        try
        {
            blockBlob.FetchAttributes();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void StateSet(string appId, string key, string state)
    {
        // Create the blob client.
        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        blobClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        // Retrieve reference to container. Containers use same name rules as tables (see table name limitations).
        var container = blobClient.GetContainerReference($"states{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if container is already created manually in Azure 
        container.CreateIfNotExists();

        var blockBlob = container.GetBlockBlobReference(key);

        blockBlob.UploadText(state);
    }

    public string StateGet(string appId, string key)
    {
        // Create the blob client.
        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        blobClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        // Retrieve reference to container. Containers use same name rules as tables (see table name limitations).
        var container = blobClient.GetContainerReference($"states{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if container is already created manually in Azure 
        container.CreateIfNotExists();

        var blockBlob = container.GetBlockBlobReference(key);

        try
        {
            return blockBlob.DownloadText();
        }
        //Azure throws exception if file not found
        catch (StorageException)
        {
            _log.Error($"StateGet, state {appId}/{key} not found");
            return null;
        }
    }

    public void StateDelete(string appId, string key)
    {
        // Create the blob client.
        var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        blobClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        // Retrieve reference to container. Containers use same name rules as tables (see table name limitations).
        var container = blobClient.GetContainerReference($"states{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if container is already created manually in Azure 
        container.CreateIfNotExists();

        var blockBlob = container.GetBlockBlobReference(key);

        try
        {
            blockBlob.Delete();
        }
        //Azure throws exception if file not found
        catch (StorageException)
        {
            _log.Error($"StateDelete, state {appId}/{key} not found");
        }
    }

    public void GameInsert(string appId, string key, string gameId, int actorNr)
    {
        var tableClient = _cloudStorageAccount.CreateCloudTableClient();
        tableClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        //table names can't have "-" in the name and may not begin with a numeric character
        var table = tableClient.GetTableReference($"games{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if table is already created manually in Azure 
        table.CreateIfNotExists();

        table.Execute(
            TableOperation.InsertOrReplace(new GameEntity {PartitionKey = key, RowKey = gameId, ActorNr = actorNr}));
    }

    public void GameDelete(string appId, string key, string gameId)
    {
        var tableClient = _cloudStorageAccount.CreateCloudTableClient();
        tableClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        //table names can't have "-" in the name and may not begin with a numeric character
        var table = tableClient.GetTableReference($"games{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if table is already created manually in Azure 
        table.CreateIfNotExists();

        try
        {
            table.Execute(TableOperation.Delete(new TableEntity {PartitionKey = key, RowKey = gameId, ETag = "*"}));
        }
        catch (StorageException)
        {
            _log.Error($"GameDelete, game {appId}/{key}/{gameId} not found");
        }
    }

    public Dictionary<string, string> GameGetAll(string appId, string key)
    {
        var tableClient = _cloudStorageAccount.CreateCloudTableClient();
        tableClient.AuthenticationScheme = AuthenticationScheme.SharedKeyLite;

        //table names can't have "-" in the name and may not begin with a numeric character
        var table = tableClient.GetTableReference($"games{appId.Replace("-", "")}");

        //create for demo - this call is obsolete if table is already created manually in Azure 
        table.CreateIfNotExists();

        var result =
            table.ExecuteQuery(
                new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key)));

        return result.ToDictionary(dynamicTableEntity => dynamicTableEntity.RowKey,
            dynamicTableEntity => dynamicTableEntity["ActorNr"].Int32Value.ToString());
    }
}

public class GameEntity : TableEntity
{
    public int ActorNr { get; set; }
}