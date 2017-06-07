#r "Newtonsoft.Json"
#load "../Models/Game.csx"
#load "../Common/CreateGame.csx"
#load "../Common/Azure.csx"
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string appId, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // Get request body
    GetGameListRequest body = await req.Content.ReadAsAsync<GetGameListRequest>();
    Azure azure = new Azure(log);

    // Set name to query string or body data
    string message;
    if (!IsValid(body, out message))
    {
        log.Error($"{req.RequestUri} - {message}");
        return req.CreateResponse(HttpStatusCode.BadRequest,
            $"{req.RequestUri} - {message}");
    }

    var list = new Dictionary<string, object>();

    foreach(var pair in azure.GameGetAll(appId, body.UserId))
    {
        var stateJson = azure.StateGet(appId, body.UserId);
        if (stateJson != null)
        {
            dynamic customProperties = null;
            if (stateJson != string.Empty)
            {
                var state = JsonConvert.DeserializeObject<dynamic>(stateJson);
                customProperties = state.CustomProperties;
            }

            GameListItem gameListItem = new GameListItem() { ActorNr = int.Parse(pair.Value), Properties = customProperties };
            list.Add(pair.Key, gameListItem);
        }
            // not exists - delete
        else
        {
            azure.GameDelete(appId, body.UserId, pair.Key);
        }
    }

    var response = new GetGameListResponse { Data = list};
    var okMsg = $"{req.RequestUri} - {JsonConvert.SerializeObject(list)}";
    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}

private static bool IsValid(GetGameListRequest request, out string message)
{
    if (string.IsNullOrEmpty(request.UserId))
    {
        message = "Missing UserId.";
        return false;
    }

    message = "";
    return true;
}