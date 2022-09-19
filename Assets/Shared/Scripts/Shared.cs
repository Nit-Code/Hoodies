using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class Shared : MonoBehaviour
{
    public const int ourMaxMessageSize = 1024;
    public const int ourMaxPlayersPerSession = 2;
    public const string ourGameOverState = "GAME_OVER";
    public const int ourTCPServerPortRangeMin = 7000;
    public const int ourTCPServerPortRangeMax = 8000;

#if UNITY_EDITOR || USE_ARGUMENTS
    public static string OurIdentityPoolId()
    {
        return CLU.GetIdentityPoolId();
    }

    public static string OurAppClientlId()
    {
        return CLU.GetAppClientId();
    }

    public static string OurUserPoolId()
    {
        return CLU.GetUserPoolId();
    }

    public static Amazon.RegionEndpoint OurRegionId()
    {
        return CLU.GetRegionId();
    }
#else

    public static string OurIdentityPoolId()
    {
        return "sa-east-1:2881276d-52c7-44a1-877e-72b05f1e8dfc";
    }

    public static string OurAppClientlId()
    {
        return "5o3te75s5ubh0brc11rjecg6lf";
    }

    public static string OurUserPoolId()
    {
        return "sa-east-1_coG0kkuWS";
    }

    public static Amazon.RegionEndpoint OurRegionId()
    {
        return Amazon.RegionEndpoint.SAEast1;
    }
#endif

    public static void Log(Object aText)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        Debug.Log(aText);
#else
        Console.WriteLine(aText);
#endif
    }

    public static void LogError(Object aText)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        Debug.LogError(aText);
#else
        Console.Error.WriteLine(aText);
#endif
    }
}
