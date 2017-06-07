#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
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
    GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();

    var okMsg = $"{req.RequestUri} - Recieved Game Join Request";
    log.Info(okMsg);
    return req.CreateResponse(HttpStatusCode.OK, okMsg);
}