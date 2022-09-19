using System;

public static class EventHandler
{
    #region SceneEvents

    public static event Action OurBeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent()
    {
        if (OurBeforeSceneUnloadEvent != null)
        {
            OurBeforeSceneUnloadEvent();
        }
    }


    public static event Action OurAfterSceneUnloadEvent;

    public static void CallAfterSceneUnloadEvent()
    {
        if (OurAfterSceneUnloadEvent != null)
        {
            OurAfterSceneUnloadEvent();
        }
    }

    public static event Action OurBeforeSceneLoadEvent;

    public static void CallBeforeSceneLoadEvent()
    {
        if (OurBeforeSceneLoadEvent != null)
        {
            OurBeforeSceneLoadEvent();
        }
    }

    public static event Action OurAfterSceneLoadEvent;

    public static void CallAfterSceneLoadEvent()
    {
        if (OurAfterSceneLoadEvent != null)
        {
            OurAfterSceneLoadEvent();
        }
    }

    public static event Action OurAfterMatchSceneLoadEvent;

    public static void CallAfterMatchSceneLoadEvent()
    {
        if (OurAfterMatchSceneLoadEvent != null)
        {
            OurAfterMatchSceneLoadEvent();
        }
    }

    public static event Action OurAfterLoggedInEvent;

    public static void CallAfterLoggedInEvent()
    {
        if (OurAfterLoggedInEvent != null)
        {
            OurAfterLoggedInEvent();
        }
    }

    public static event Action OurAfterLoggedOutEvent;

    public static void CallAfterLoggedOutEvent()
    {
        if (OurAfterLoggedOutEvent != null)
        {
            OurAfterLoggedOutEvent();
        }
    }
    #endregion

    #region MatchEvents

    public static event Action OurMatchStartupEvent;

    public static void CallMatchStartupEvent()
    {
        if (OurMatchStartupEvent != null)
        {
            OurMatchStartupEvent();
        }
    }

    public static event Action OurStartCountdownEvent;

    public static void CallStartCountdownEvent()
    {
        if (OurStartCountdownEvent != null)
        {
            OurStartCountdownEvent();
        }
    }

    public static event Action OurSpawnUnitEvent;

    public static void CallSpawnUnitEvent() //EXAMPLE
    {
        if (OurSpawnUnitEvent != null)
        {
            OurSpawnUnitEvent();
        }
    }


    public static event Action OurStartTurnEvent;

    public static void CallStartTurnEvent()
    {
        if (OurStartTurnEvent != null)
        {
            OurStartTurnEvent();
        }
    }
    #endregion
}
