#r "Newtonsoft.Json"
#load "../Models/Game.csx"
#load "CreateGame.csx"
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class HubMessage
{
    public static string WrapMessage(Dictionary<string, string> notificationContent, string username, string usertag,
        string target, string appid)
    {
        var conditions = new List<IList<string>>
            {
                new[] {usertag, "EQ", target},
                new[] {"PhotonAppId", "EQ", appid}
            };

        var content = new Dictionary<string, string>();
        foreach (var item in notificationContent)
        {
            content[item.Key] = item.Value.Replace("{USERNAME}", username);
        }

        var notifications = new List<HubNotification>
            {
                new HubNotification
                {
                    SendDate = DateTime.UtcNow.ToShortTimeString(),
                    IgnoreUserTimezone = true,
                    Content = content,
                }
            };
        var request = new Dictionary<string, HubRequest>
            {
                {
                    "request",
                    new HubRequest
                    {
                        Notifications = notifications,
                        Conditions = conditions,
                    }
                }
            };
        return JsonConvert.SerializeObject(request);
    }
}

public class HubNotification
{
    public string SendDate { get; set; }
    public bool IgnoreUserTimezone { get; set; }
    public Dictionary<string, string> Content { get; set; }
}

public class HubRequest
{
    public List<HubNotification> Notifications { get; set; }
    public List<IList<string>> Conditions { get; set; }
}