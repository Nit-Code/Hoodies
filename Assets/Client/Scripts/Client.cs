public class Client
{
    // These only make sense after a server has been built and uploaded to gamelift

#if UNITY_EDITOR || USE_ARGUMENTS
    public static string GetFleetId()
    {
        return CLU.GetFleetId();
    }
#else
    public static string GetFleetId()
    {
        return "fleet-3f836750-bc35-42bb-ab0d-9e3d9bff5e57";
    }
#endif

}