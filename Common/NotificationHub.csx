#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#r "Microsoft.Azure.NotificationHubs"
#load "../Models/Game.csx"
#load "HubMessage.csx"
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.NotificationHubs;

public class NotificationHub
{
    private readonly NotificationHubClient _hub;
    private readonly TraceWriter _log;

    public NotificationHub(TraceWriter log)
    {
        _log = log;
        var conn = System.Configuration.ConfigurationManager.ConnectionStrings["AzureNotificationHub"].ConnectionString;
        _log.Info($"Connection String = {conn}");
        _hub = NotificationHubClient.CreateClientFromConnectionString(conn, "notifyhub");

    }

    public async Task SendMessage(Dictionary<string, string> notificationContent, string username, string usertag,
        string target, string appid)
    {
        var template = HubMessage.WrapMessage(notificationContent, username, usertag, target, appid);
        //implement logic to send notification here
    }
}