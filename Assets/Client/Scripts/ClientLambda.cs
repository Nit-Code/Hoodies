using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using UnityLambdaInputOutput;
using Newtonsoft.Json;
using System.Text;

public class ClientLambda : MonoBehaviour
{
    AmazonLambdaClient myLambdaClient;
    AuthenticationManager myAuthenticationManagerReference;

    private int myToLoad;
    private int myLoaded;

    private void Awake()
    {
        myToLoad = 1;
        myLoaded = 0;

        if (TryGetComponent<AuthenticationManager>(out myAuthenticationManagerReference))
            myLoaded++;

        if (!IsReadyToStart())
            Shared.LogError("[HOOD][CLIENT][LAMBDA] - Not Ready.");

        EventHandler.OurAfterLoggedInEvent += SetupLambdaClient;
    }

    private bool IsReadyToStart()
    {
        return myToLoad == myLoaded;
    }

    public void SetupLambdaClient() 
    {
        myLambdaClient = new AmazonLambdaClient(myAuthenticationManagerReference.GetCredentials(), Shared.OurRegionId());
        EventHandler.OurAfterLoggedInEvent -= SetupLambdaClient;
        EventHandler.OurAfterLoggedOutEvent += ClearLambdaClient;
    }

    public void ClearLambdaClient()
    {
        myLambdaClient = null;
        EventHandler.OurAfterLoggedOutEvent -= ClearLambdaClient;
        EventHandler.OurAfterLoggedInEvent += SetupLambdaClient;
    }

    public async void InvokeBasicFunction() 
    {
        if (myLambdaClient == null)
        {
            Shared.LogError("[HOOD][CLIENT][LAMBDA] InvokeBasicFunction");
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
        Shared.Log("[HOOD][CLIENT][LAMBDA] Response statusCode: " + invokeResponse.StatusCode);

        if (invokeResponse.StatusCode == 200)
        {
            string payloadAsString = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
            BasicFunctionOutput outputAsObject = JsonConvert.DeserializeObject<BasicFunctionOutput>(payloadAsString);
            if (outputAsObject != null)
            {
                Shared.Log("[HOOD][CLIENT][LAMBDA] Response sucess: " + outputAsObject.Value);
            }
        }
    }

    public async void InvokeGetGameSessionId(string aShortLobbyId, GameLiftClient aGameLiftClient, MenuSceneUIManager aMenuScene)
    {
        if (myLambdaClient == null || string.IsNullOrEmpty(aShortLobbyId))
        {
            Shared.LogError("[HOOD][CLIENT][LAMBDA] InvokeGetGameSessionId");
            return;
        }

        GetGameSessionIdInput input = new GetGameSessionIdInput();
        input.ShortLobbyId = aShortLobbyId;
        input.LogsEnabled = true;

        string inputAsString = JsonConvert.SerializeObject(input);

        InvokeRequest invokeRequest = new InvokeRequest
        {
            FunctionName = LambdaNames.ourGetGameSessionIdName,
            InvocationType = InvocationType.RequestResponse,
            Payload = inputAsString
        };

        InvokeResponse invokeResponse = await myLambdaClient.InvokeAsync(invokeRequest);
        Shared.Log("[HOOD][CLIENT][LAMBDA] Response statusCode: " + invokeResponse.StatusCode);

        if (invokeResponse.StatusCode == 200)
        {
            string payloadAsString = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
            GetGameSessionIdOutput outputAsObject = JsonConvert.DeserializeObject<GetGameSessionIdOutput>(payloadAsString);
            if (outputAsObject != null && outputAsObject.Success) 
            {
                aGameLiftClient.CreateOrJoinMatch("PRIVATE_GUEST", outputAsObject.GamesSessionId);
                Shared.Log("[HOOD][CLIENT][LAMBDA] Response successful! got gameSessionId: " + outputAsObject.GamesSessionId);
                return;
            }
        }

        Shared.LogError("[HOOD][CLIENT][LAMBDA] Response unsuccessful!, something went wrong while fetching a game session id.");
        aMenuScene.OnPlayerSessionCreationFailed();
    }
}
