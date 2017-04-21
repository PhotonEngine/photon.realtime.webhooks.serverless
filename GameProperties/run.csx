#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "../Models/Game.csx"
#load "../Common/CreateGame.csx"
#load "../Common/Azure.csx"
#load "../Common/NotificationHub.csx"
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
    GamePropertiesRequest body = await req.Content.ReadAsAsync<GamePropertiesRequest>();
    Azure azure = new Azure(log);
    NotificationHub hub = new NotificationHub(log);

    // Set name to query string or body data
    string message;
    if (!IsValid(body, out message))
    {
        log.Error($"{req.RequestUri} - {message}");
        return req.CreateResponse(HttpStatusCode.BadRequest,
            $"{req.RequestUri} - {message}");
    }
    if(body.State != null)
    { 
        var state = (string)JsonConvert.SerializeObject(body.State);
        azure.StateSet(appId, body.GameId, state);

        var properties = body.Properties;
        object actorNrNext = null;
        properties?.TryGetValue("turn", out actorNrNext);
        var userNextInTurn = string.Empty;
        foreach (var actor in body.State.ActorList)
        {
            if (actorNrNext != null)
            {
                if (actor.ActorNr == actorNrNext)
                {
                    userNextInTurn = (string)actor.UserId;
                }
            }
            azure.GameInsert(appId, (string)actor.UserId, body.GameId, (int)actor.ActorNr);
        }

        if (!string.IsNullOrEmpty(userNextInTurn))
        {
            var notificationContent = new Dictionary<string, string>
                                                      {
                                                          { "en", "{USERNAME} finished. It's your turn." },
                                                          { "de", "{USERNAME} hat seinen Zug gemacht. Du bist dran." },
                                                      };
            hub.SendMessage(notificationContent, body.Username, "UID2", userNextInTurn, appId);
        }
    }

    var okMsg = $"{req.RequestUri} - Uploaded Game Properties";
    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}

private static bool IsValid(GamePropertiesRequest request, out string message)
{
    if (string.IsNullOrEmpty(request.GameId))
    {
        message = "Missing GameId.";
        return false;
    }

    message = "";
    return true;
}