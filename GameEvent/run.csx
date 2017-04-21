#r "Newtonsoft.Json"
#load "../Models/Game.csx"
using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string appId, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
    // Get request body
    GameEventRequest body = await req.Content.ReadAsAsync<GameEventRequest>();

    var okMsg = $"{req.RequestUri} - Recieved Game Event";
    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}