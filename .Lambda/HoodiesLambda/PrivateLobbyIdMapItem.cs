namespace HoodiesLambda
{
    public class PrivateLobbyIdMapItem
    {
        public int HoodId { get; set; }
        public bool IsPrivateLobbyIdActive { get; set; }
        public string LongPrivateLobbyId { get; set; }
        public string PrivateLobbyIdCreationTime { get; set; }
        public string ShortPrivateLobbyId { get; set; }

        public PrivateLobbyIdMapItem()
        {
            HoodId = -1;
            IsPrivateLobbyIdActive = false;
            LongPrivateLobbyId = "";
            PrivateLobbyIdCreationTime = "";
            ShortPrivateLobbyId = "";
        }
    }
}
