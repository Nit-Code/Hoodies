using Newtonsoft.Json;
/*
    These classes are type wrappers to aid with ClientLambda's expected serializtion/deserialization process
    ClientLambda streams expects members to have names so we use JsonPropertyName attibute from System.Text.Json.Serialization
    System.Text.Json.Serialization is incompatible with unity's Newtonsoft.Json
    For the sake of not importing a million different dll to deal with very standard serialization, 
    the clases in this namespace must be manually synced with the same named classes in namespace AWSLambdaInputOutput
 */

namespace UnityLambdaInputOutput
{
    public class LambdaNames 
    {
        public const string ourBasicFunctionName = "BasicFunction";
        public const string ourGetGameSessionIdName = "GetGameSessionId";
        public const string ourCreateShortLobbyIdName = "CreateShortLobbyId";
    }

    #region BasicFunction IO
    [System.Serializable]
    public class BasicFunctionInput
    {
        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    [System.Serializable]
    public class BasicFunctionOutput
    {
        [JsonProperty(PropertyName = "Success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }
    }
    #endregion

    #region GetGameSessionId IO
    [System.Serializable]
    public class GetGameSessionIdInput
    {
        [JsonProperty(PropertyName = "ShortLobbyId")]
        public string ShortLobbyId { get; set; }

        [JsonProperty(PropertyName = "LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    [System.Serializable]
    public class GetGameSessionIdOutput
    {
        [JsonProperty(PropertyName = "Success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "FailReason")]
        public string FailReason { get; set; }

        [JsonProperty(PropertyName = "GamesSessionId")]
        public string GamesSessionId { get; set; }
    }
    #endregion

    #region CreateShortLobbyId IO
    [System.Serializable]
    public class CreateShortLobbyIdInput
    {
        [JsonProperty(PropertyName = "HoodId")]
        public int HoodId { get; set; }

        [JsonProperty(PropertyName = "GamesSessionId")]
        public string GamesSessionId { get; set; }

        [JsonProperty(PropertyName = "LogsEnabled")]
        public bool LogsEnabled { get; set; }
    }

    [System.Serializable]
    public class CreateShortLobbyIdOutput
    {
        [JsonProperty(PropertyName = "Success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "ShortLobbyId")]
        public string ShortLobbyId { get; set; }
    }
    #endregion
}

