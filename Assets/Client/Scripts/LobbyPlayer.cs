using System.Collections.Generic;

public class LobbyPlayer
{
    public string myPlayerSessionId { get; set; }
    public string myName { get; set; }
    public List<string> myDecks { get; set; }
    public bool myIsConnected { get; set; }
    public string mySelectedDeckId { get; set; }
    public bool myIsReady { get; set; }
    public LobbyPlayer()
    {

    }
}
