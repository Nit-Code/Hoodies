using UnityEngine;
using System.Collections.Generic;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System;
using System.Threading.Tasks;
using System.Net;

public class AuthenticationManager : MonoBehaviour
{
    private AmazonCognitoIdentityProviderClient myProvider;
    private CognitoAWSCredentials myCognitoAWSCredentials;
    private static string myUserId = "";
    private CognitoUser myUser;
    private SharedUser myUserReference;

    private void Awake()
    {
        Debug.Log("[HOOD][CLIENT][AUTH] - AuthenticationManager/Awake");

        myProvider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), Shared.OurRegionId());

        if (!TryGetComponent<SharedUser>(out myUserReference))
        {
            Debug.LogError("[HOOD][CLIENT][AUTH] - Data not found");
        }
    }

    public async Task<bool> RefreshSession()
    {
        Debug.Log("[HOOD][CLIENT][AUTH] - AuthenticationManager/RefreshSession");

        DateTime issued = DateTime.Now;
        SessionCache userSessionCache = new SessionCache();
        SaveDataManager.LoadJsonData(userSessionCache);
        if (string.IsNullOrEmpty(userSessionCache.myRefreshToken)) 
        {
            return false;
        }

        try
        {
            CognitoUserPool userPool = new CognitoUserPool(Shared.OurUserPoolId(), Shared.OurAppClientlId(), myProvider);

            // apparently the username field can be left blank for a token refresh request
            CognitoUser user = new CognitoUser("", Shared.OurAppClientlId(), userPool, myProvider);

            // The "Refresh token expiration (days)" (Cognito->UserPool->General Settings->App clients->Show Details) is the
            // amount of time since the last login that you can use the refresh token to get new tokens. After that period the refresh
            // will fail Using DateTime.Now.AddHours(1) is a workaround for https://github.com/aws/aws-sdk-net-extensions-cognito/issues/24
            user.SessionTokens = new CognitoUserSession(
                userSessionCache.myIdToken,
                userSessionCache.myAccessToken,
                userSessionCache.myRefreshToken,
                issued,
                DateTime.Now.AddDays(30)); // TODO: need to investigate further. 
                                            // It was my understanding that this should be set to when your refresh token expires...

            // Attempt refresh token call
            AuthFlowResponse authFlowResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            })
            .ConfigureAwait(false);
            Debug.Log("[HOOD][CLIENT][AUTH] - User refresh token successfully updated!");

            // update session cache
            SessionCache userSessionCacheToUpdate = new SessionCache(
                authFlowResponse.AuthenticationResult.IdToken,
                authFlowResponse.AuthenticationResult.AccessToken,
                authFlowResponse.AuthenticationResult.RefreshToken,
                userSessionCache.myUserId,
                userSessionCache.myUsername);

            myUserReference.SetSessionCache(userSessionCacheToUpdate);

            // update credentials with the latest access token
            myCognitoAWSCredentials = user.GetCognitoAWSCredentials(Shared.OurIdentityPoolId(), Shared.OurRegionId());
            myUserReference.SetCognitoCredentials(myCognitoAWSCredentials);
            myUser = user;
            EventHandler.CallAfterLoggedInEvent();
            return true;
        }
        catch (NotAuthorizedException ne)
        {
            // https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-with-identity-providers.html
            // refresh tokens will expire - user must login manually every x days (see user pool -> app clients -> details)
            Debug.Log("[HOOD][CLIENT][AUTH] - Error Message: " + ne.Message + " - Error Code: " + ne.ErrorCode.ToString());
        }
        catch (WebException webEx)
        {
            // we get a web exception when we cant connect to aws - means we are offline
            Debug.Log("[HOOD][CLIENT][AUTH] - Error Message: " + webEx.Message);
        }
        catch (Exception ex)
        {
            Debug.Log("[HOOD][CLIENT][AUTH] - Error Message: " + ex.Message);
        }
        
        return false;
    }

    public async Task<bool> Login(string email, string password)
    {
        CognitoUserPool userPool = new CognitoUserPool(Shared.OurUserPoolId(), Shared.OurAppClientlId(), myProvider);
        CognitoUser user = new CognitoUser(email, Shared.OurAppClientlId(), userPool, myProvider);

        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        try
        {
            AuthFlowResponse authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
            myUserId = await GetUserIdFromProvider(authFlowResponse.AuthenticationResult.AccessToken);
            string userName = await GetUserNameFromProvider(authFlowResponse.AuthenticationResult.AccessToken);

            SessionCache userSessionCache = new SessionCache(
               authFlowResponse.AuthenticationResult.IdToken,
               authFlowResponse.AuthenticationResult.AccessToken,
               authFlowResponse.AuthenticationResult.RefreshToken,
               myUserId,
               userName);

            myUserReference.SetSessionCache(userSessionCache);

            // This how you get credentials to use for accessing other services.
            // This IdentityPool is your Authorization, so if you tried to access using an
            // IdentityPool that didn't have the policy to access your target AWS service, it would fail.
            myCognitoAWSCredentials = user.GetCognitoAWSCredentials(Shared.OurIdentityPoolId(), Shared.OurRegionId());
            myUserReference.SetCognitoCredentials(myCognitoAWSCredentials);
            myUser = user;
            Debug.Log("[HOOD][CLIENT][AUTH] - Logged in successfully. UserId: " + userSessionCache.myUserId + "Username: " + userSessionCache.myUsername);
            EventHandler.CallAfterLoggedInEvent();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("[HOOD][CLIENT][AUTH] - Error Message: " + e.Message);
            return false;
        }
    }

    public async Task<bool> Signup(string username, string email, string password)
    {
        string appClientId = Shared.OurAppClientlId();
        SignUpRequest signUpRequest = new SignUpRequest()
        {
            ClientId = appClientId,
            Username = email,
            Password = password
        };

        // must provide all attributes required by the User Pool that you configured
        List<AttributeType> attributes = new List<AttributeType>()
        {
            new AttributeType(){
            Name = "email", Value = email
            },
            new AttributeType(){
            Name = "preferred_username", Value = username
            }
        };
        signUpRequest.UserAttributes = attributes;

        try
        {
            SignUpResponse sighupResponse = await myProvider.SignUpAsync(signUpRequest);
            Debug.Log("[HOOD][CLIENT][AUTH] - Sign up successful");
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("[HOOD][CLIENT][AUTH] - Sign up failed, exception:" + e.Message);
            return false;
        }
    }

    // we call this once after the user is authenticated, then cache it as part of the session for later retrieval 
    private async Task<string> GetUserIdFromProvider(string accessToken)
    {
        // Debug.Log("Getting user's id...");
        string subId = "";

        Task<GetUserResponse> responseTask =
           myProvider.GetUserAsync(new GetUserRequest
           {
               AccessToken = accessToken
           });

        GetUserResponse responseObject = await responseTask;

        // set the user id
        foreach (var attribute in responseObject.UserAttributes)
        {
            if (attribute.Name == "sub")
            {
                subId = attribute.Value;
                break;
            }
        }

        return subId;
    }

    private async Task<string> GetUserNameFromProvider(string accessToken)
    {
        string name = "";

        Task<GetUserResponse> responseTask =
           myProvider.GetUserAsync(new GetUserRequest
           {
               AccessToken = accessToken
           });

        GetUserResponse responseObject = await responseTask;

        foreach (var attribute in responseObject.UserAttributes)
        {
            if (attribute.Name == "preferred_username")
            {
                name = attribute.Value;
                break;
            }
        }

        return name;
    }

    // Limitation note: so this GlobalSignOutAsync signs out the user from ALL devices, and not just the game.
    // So if you had other sessions for your website or app, those would also be killed.  
    // Currently, I don't think there is native support for granular session invalidation without some work arounds.
    public async void SignOut()
    {
        await myUser.GlobalSignOutAsync();

        // Important! Make sure to remove the local stored tokens 
        myUserReference.CleanSessionCache();
        EventHandler.CallAfterLoggedOutEvent();

        Debug.Log("[HOOD][CLIENT][AUTH] - User Logged out");
        //TODO: cant login after logout with any user
    }

    // access to the user's authenticated credentials to be used to call other AWS APIs
    public CognitoAWSCredentials GetCredentials()
    {
        return myCognitoAWSCredentials;
    }
}
