using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using UnityLambdaInputOutput;
using Newtonsoft.Json;
using System.Text;
using Amazon.CognitoIdentity;
using SharedScripts;
using Assets.Shared.Scripts.Messages.Server;
using System;

public class ServerLambda : MonoBehaviour
{
    AmazonLambdaClient myLambdaClient;
    NetworkServer myNetworkServerReference;

    private void Awake()
    {
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(CLU.GetIdentityPoolAccountId(), CLU.GetIdentityPoolId(), "", CLU.GetIdentityPoolAuthArn(), CLU.GetRegionId());
        myLambdaClient = new AmazonLambdaClient(credentials, CLU.GetRegionId());

        if (!TryGetComponent<NetworkServer>(out myNetworkServerReference))
            Shared.LogError("[HOOD][SERVER][NETWORK] - NetworkServer Component not found");
    }

    public async void InvokeBasicFunction()
    {
        if (myLambdaClient == null) 
        {
            Shared.LogError("[HOOD][SERVER][LAMBDA] InvokeBasicFunction");
            return;
        }

        BasicFunctionInput input = new BasicFunctionInput();
        input.Value = "Some Value";
        input.LogsEnabled = true;

        string inputAsString = JsonConvert.SerializeObject(input);

        InvokeRequest invokeRequest = new InvokeRequest
        {
            FunctionName = LambdaNames.ourBasicFunctionName,
            InvocationType = InvocationType.RequestResponse,
            Payload = inputAsString
        };

        InvokeResponse invokeResponse = await myLambdaClient.InvokeAsync(invokeRequest);
        Shared.Log("[HOOD][SERVER][LAMBDA] Response statusCode: " + invokeResponse.StatusCode);

        if (invokeResponse.StatusCode == 200)
        {
            string payloadAsString = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
            BasicFunctionOutput outputAsObject = JsonConvert.DeserializeObject<BasicFunctionOutput>(payloadAsString);
            if (outputAsObject != null)
            {
                Shared.Log("[HOOD][SERVER][LAMBDA] Response sucess: " + outputAsObject.Value);
            }
        }
    }

    public async void InvokeCreateShortLobbyId(int aHoodId, string aGameSessionId, int aHostConnectionId)
    {
        if (!CLU.CreateShortLobbyIdEnabled()) 
        {
            return;
        }
        Shared.Log("[HOOD][SERVER][LAMBDA] InvokeCreateShortLobbyId");

        if (myLambdaClient == null)
        {
            Shared.LogError("[HOOD][SERVER][LAMBDA] InvokeCreateShortLobbyId");
            return;
        }

        CreateShortLobbyIdInput input = new CreateShortLobbyIdInput();
        input.HoodId = aHoodId;
        input.GamesSessionId = aGameSessionId;
        input.LogsEnabled = true;

        string inputAsString = JsonConvert.SerializeObject(input);

        InvokeRequest invokeRequest = new InvokeRequest
        {
            FunctionName = LambdaNames.ourCreateShortLobbyIdName,
            InvocationType = InvocationType.RequestResponse,
            Payload = inputAsString
        };

        try
        {
            InvokeResponse invokeResponse = await myLambdaClient.InvokeAsync(invokeRequest);
            Shared.Log("[HOOD][SERVER][LAMBDA] Response statusCode: " + invokeResponse.StatusCode);

            if (invokeResponse.StatusCode == 200)
            {
                string payloadAsString = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
                CreateShortLobbyIdOutput outputAsObject = JsonConvert.DeserializeObject<CreateShortLobbyIdOutput>(payloadAsString);
                if (outputAsObject != null)
                {
                    SharedServerMessage responseMessage;
                    if (outputAsObject.Success)
                    {
                        // aHoodId : 1 = true
                        responseMessage = new ServerDatabaseMessage(DatabaseMessageId.SEND_HOST_SHORT_LOBBY_ID, 1, outputAsObject.ShortLobbyId);
                        myNetworkServerReference.SendMessageToPlayer(aHostConnectionId, responseMessage);
                    }
                    else
                    {
                        // aHoodId : -1 = false
                        responseMessage = new ServerDatabaseMessage(DatabaseMessageId.SEND_HOST_SHORT_LOBBY_ID, -1, "");
                        myNetworkServerReference.SendMessageToPlayer(aHostConnectionId, responseMessage);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Shared.LogError("[HOOD][SERVER][LAMBDA] Exception on invoke: " + ex.Message);
            // TODO: inform player 
            myNetworkServerReference.ShutDownGameSession(); //disconnect them and shut down game session
        }
    }
}
