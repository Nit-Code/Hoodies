using UnityEngine;
using Oddworm.Framework;

public class CLU : MonoBehaviour
{
    public enum LOG_FLOW
    {
        UNDEFINED,
        PRIVATE_HOST,
        PRIVATE_GUEST
    }

    enum REGION_ID
    {
        UNDEFINED,
        SAEast1
    }

#if UNITY_SERVER
#if UNITY_EDITOR
    private const string myArgsFile = "EditorServerArguments.txt";
#else
    private const string myArgsFile = "ServerArguments.txt";
#endif
#else
#if UNITY_EDITOR
    private const string myArgsFile = "EditorClientArguments.txt";
#else
    private const string myArgsFile = "ClientArguments.txt";
#endif
#endif

    public static bool GetIsLobbyCacheEnabled()
    {
        return CommandLine.GetBool("-LOBBY_CACHE_ENABLED", false);
    }

    public static string GetSessionCacheFilename()
    {
        return CommandLine.GetString("-SESSION_CACHE_FILENAME", "");
    }

    public static string GetFleetId()
    {
        return CommandLine.GetString("-FLEET_ID", "");
    }

    public static LOG_FLOW GetLogFlow()
    {
        return CommandLine.GetEnum<LOG_FLOW>("-LOG_FLOW", LOG_FLOW.UNDEFINED);
    }

    public static string GetIdentityPoolId()
    {
        return CommandLine.GetString("-IDENTITY_POOL_ID", "");
    }

    public static string GetIdentityPoolAccountId()
    {
        return CommandLine.GetString("-IDENTITY_POOL_ACCOUNT_ID", "");
    }

    public static string GetIdentityPoolAuthArn()
    {
        return CommandLine.GetString("-IDENTITY_POOL_ID_AUTH_ARN", "");
    }

    public static string GetIdentityPoolUnauthArn()
    {
        return CommandLine.GetString("-IDENTITY_POOL_ID_UNAUTH_ARN", "");
    }

    public static string GetAppClientId()
    {
        return CommandLine.GetString("-APP_CLIENT_ID", "");
    }

    public static string GetUserPoolId()
    {
        return CommandLine.GetString("-USER_POOL_ID", "");
    }

    public static Amazon.RegionEndpoint GetRegionId()
    {
        REGION_ID region = CommandLine.GetEnum<REGION_ID>("-REGION_ID", REGION_ID.UNDEFINED);
        switch (region)
        {
            case REGION_ID.SAEast1:
                return Amazon.RegionEndpoint.SAEast1;
            case REGION_ID.UNDEFINED:
            default:
                return null;
        }
    }

    public static bool GetIsConnectLocalEnabled()
    {
#if UNITY_SERVER && UNITY_STANDALONE_LINUX && !UNITY_EDITOR 
        //linux servers not running in editor, never use CONNECT_LOCAL parameter
        return false;
#else
        //clients, windows servers and linux servers running in the editor may use CONNECT_LOCAL parameter
        return CommandLine.GetBool("-CONNECT_LOCAL", false);
#endif
    }
    public static bool CreateShortLobbyIdEnabled()
    {
#if UNITY_SERVER && UNITY_STANDALONE_LINUX && !UNITY_EDITOR 
        // linux servers not running in editor, always invoke use short lobby id usage
        return true;
#else
        //clients, windows servers and linux servers running in the editor may not invoke short lobby id usage
        return CommandLine.GetBool("-SHORT_LOBBY_ID", false);
#endif
    }

    public static int GetConnectLocalServerPort()
    {
        return CommandLine.GetInt("-CONNECT_LOCAL_SERVER_PORT", -1);
    }

    public static string GetConnectLocalServerIP()
    {
        return CommandLine.GetString("-CONNECT_LOCAL_SERVER_IP", "");
    }

    public static bool GetIsLocalServerURLSet()
    {
        return (!string.IsNullOrEmpty(GetConnectLocalServerIP()) && GetConnectLocalServerPort() != -1);
    }

    // On many platforms you can simply use System.IO to load a file as shown below.
    // On Android you can't use System.IO though, you need to use UnityWebRequest instead.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void LoadCommandLine()
    {
        // Use commandline options passed to the application
        var text = System.Environment.CommandLine + "\n";

        // Load the commandline file content.
        // You need to adjust the path to where the file is located in your project.

        var path = System.IO.Path.Combine(Application.streamingAssetsPath, myArgsFile);

        if (System.IO.File.Exists(path))
        {
            text += System.IO.File.ReadAllText(path);
        }
        else
        {
#if UNITY_EDITOR || USE_ARGUMENTS
            Shared.LogError("[HOOD][CLU] Could not find commandline file: " + path);
#endif
        }

        // Initialize the CommandLine
        Oddworm.Framework.CommandLine.Init(text);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("File/Open Arguments File", priority = 1001)]
    static void OpenCommandLineMenuItem()
    {
        // The CommandLine.txt file location
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, myArgsFile);

        // If the directory does not exist, create it.
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        // If the CommandLine.txt does not exist, create it.
        if (!System.IO.File.Exists(path))
        {
            System.IO.File.WriteAllText(path, "");
            UnityEditor.AssetDatabase.Refresh();
        }

        // Open the CommandLine.txt file
        UnityEditor.EditorUtility.OpenWithDefaultApp(path);
    }
#endif

#if UNITY_EDITOR
    [UnityEditor.MenuItem("File/Open Arguments Folder", priority = 1000)]
    static void OpenFolder()
    {
        // The CommandLine.txt file location
        var path = Application.streamingAssetsPath;

        // If the directory does not exist, create it.
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        // Open the CommandLine.txt file
        UnityEditor.EditorUtility.OpenWithDefaultApp(path);
    }
#endif
}