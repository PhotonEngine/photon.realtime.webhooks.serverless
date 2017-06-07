#r "Newtonsoft.Json"
#load "../Models/Game.csx"
#load "../Common/CreateGame.csx"
#load "../Common/Azure.csx"
using System;
using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string appId, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // Get request body
    GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();
    Azure azure = new Azure(log);

    // Set name to query string or body data
    string message;
    if (!CreateGame.IsValid(body, out message))
    {
        log.Error($"{req.RequestUri} - {message}");
        return req.CreateResponse(HttpStatusCode.BadRequest,
            $"{req.RequestUri} - {message}");
    }

    dynamic response;
    if(!string.IsNullOrEmpty(body.Type) && body.Type == "Load")
    {
        response = CreateGame.GameLoad(body, appId, azure);
    }
    else
    {
        response = CreateGame.GameCreate(body, appId, azure);
    }

    var okMsg = $"{req.RequestUri} - {JsonConvert.SerializeObject(response)}";
    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}