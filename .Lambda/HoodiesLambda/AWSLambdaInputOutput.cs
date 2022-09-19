using System.Text.Json.Serialization;

/*
    These classes are type wrappers to aid with Lambda's expected serializtion/deserialization process
    Lambda streams expects members to have names so we use JsonPropertyName attibute from System.Text.Json.Serialization
    System.Text.Json.Serialization is incompatible with unity's Newtonsoft.Json
    For the sake of not importing a million different dll to deal with very standard serialization, 
    the clases in this namespace must be manually synced with the same named classes in namespace UnityLambdaInputOutput
 */
namespace AWSLambdaInputOutput
{
    #region BasicFunction IO
    public class BasicFunctionInput
    {
        [JsonPropertyName("Value")]
        public string? Value { get; set; }

        [JsonPropertyName("LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    public class BasicFunctionOutput
    {
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }
    #endregion

    #region GetGameSessionId IO
    public class GetGameSessionIdInput
    {
        [JsonPropertyName("ShortLobbyId")]
        public string? ShortLobbyId { get; set; }

        [JsonPropertyName("LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    public class GetGameSessionIdOutput
    {
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("FailReason")]
        public string? FailReason { get; set; }

        [JsonPropertyName("GamesSessionId")]
        public string? GamesSessionId { get; set; }
    }
    #endregion

    #region CreateShortLobbyId IO
    public class CreateShortLobbyIdInput
    {
        [JsonPropertyName("HoodId")]
        public int HoodId { get; set; }

        [JsonPropertyName("GamesSessionId")]
        public string? GamesSessionId { get; set; }

        [JsonPropertyName("LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    public class CreateShortLobbyIdOutput
    {
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("ShortLobbyId")]
        public string? ShortLobbyId { get; set; }
    }
    #endregion
}
