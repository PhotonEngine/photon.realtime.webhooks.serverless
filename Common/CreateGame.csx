#r "Newtonsoft.Json"
#load "../Models/Game.csx"
#load "../Common/Azure.csx"
using System;
using System.Net;
using Newtonsoft.Json;

private static class CreateGame
{
    public static dynamic GameCreate(GameCreateRequest request, string appId, Azure DataAccess)
    {
        dynamic response;
        if (DataAccess.StateExists(appId, request.GameId))
        {
            response = new ErrorResponse { Message = "Game already exists." };
            return response;
        }

        if (request.CreateOptions == null)
        {
            DataAccess.StateSet(appId, request.GameId, string.Empty);
        }
        else
        {
            DataAccess.StateSet(appId, request.GameId, (string)JsonConvert.SerializeObject(request.CreateOptions));
        }

        response = new OkResponse();
        //_logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
        return response;
    }

    public static dynamic GameLoad(GameCreateRequest request, string appId, Azure DataAccess)
    {
        dynamic response;
        var stateJson = DataAccess.StateGet(appId, request.GameId);

        if (!string.IsNullOrEmpty(stateJson))
        {
            response = new GameLoadResponse { State = JsonConvert.DeserializeObject(stateJson) };
            return response;
        }
        //TBD - check how deleteIfEmpty works with createifnot exists
        if (stateJson == string.Empty)
        {
            DataAccess.StateDelete(appId, request.GameId);

            //_logger.LogInformation($"Deleted empty state, app id {appId}, gameId {request.GameId}");

        }

        if (request.CreateIfNotExists)
        {
            response = new OkResponse();
            //_logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
            return response;
        }

        response = new ErrorResponse { Message = "GameId not Found." };
        //_logger.LogError($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
        return response;
    }
    public static bool IsValid(GameCreateRequest request, out string message)
    {
        if (string.IsNullOrEmpty(request.GameId))
        {
            message = "Missing GameId.";
            return false;
        }

        if (string.IsNullOrEmpty(request.UserId))
        {
            message = "Missing UserId.";
            return false;
        }

        message = "";
        return true;
    }
}