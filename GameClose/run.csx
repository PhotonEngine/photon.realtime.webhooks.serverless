#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "../Models/Game.csx"
#load "../Common/CreateGame.csx"
#load "../Common/Azure.csx"
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string appId, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // Get request body
    GameCloseRequest body = await req.Content.ReadAsAsync<GameCloseRequest>();
    Azure azure = new Azure(log);

    // Set name to query string or body data
    string message;
    var okMsg = $"{req.RequestUri} - Closed Game - {body.GameId}";
    if (!IsValid(body, out message))
    {
        var errorMsg = $"{req.RequestUri} - {message}";
        log.Error(errorMsg);
        return req.CreateResponse(HttpStatusCode.BadRequest, errorMsg);
    }

    if (body.State == null)
    {
        if(body.ActorCount > 0)
        {
            var errorMsg = $"{req.RequestUri} - Missing State.";
            log.Error(errorMsg);
            return req.CreateResponse(HttpStatusCode.BadRequest, errorMsg);
        }
    
        azure.StateDelete(appId, body.GameId);
        log.Info(okMsg);
        return req.CreateResponse(HttpStatusCode.OK, okMsg);
    }

    foreach(var actor in body.State.ActorList)
    {
        azure.GameInsert(appId, (string)actor.UserId, body.GameId, (int)actor.ActorNr);
    }

    //deprecated
    if (body.State2 != null)
    {
        foreach (var actor in body.State2.ActorList)
        {
            azure.GameInsert(appId, (string)actor.UserId, body.GameId, (int)actor.ActorNr);
        }
    }

    var state = (string)JsonConvert.SerializeObject(body.State);
    azure.StateSet(appId, body.GameId, state);

    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}

private static bool IsValid(GameCloseRequest request, out string message)
{
if (string.IsNullOrEmpty(request.GameId))
{
message = "Missing GameId.";
return false;
}

message = "";
return true;
}