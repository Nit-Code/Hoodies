using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using System.Text.Json.Serialization;
using System.Globalization;
using AWSLambdaInputOutput;

namespace HoodiesLambda
{
    public class GetGameSessionId
    {
        private const string SEPARATOR = "_";

        public async Task<GetGameSessionIdOutput> GetGameSessionIdHandler(GetGameSessionIdInput input, ILambdaContext context)
        {
            if (input.LogsEnabled)
                context.Logger.Log("[HOOD][LAMBDA] - GetGameSessionIdHandler");

            GetGameSessionIdOutput output = new GetGameSessionIdOutput();
            output.Success = false;

            if (string.IsNullOrEmpty(input.ShortLobbyId))
            {
                output.FailReason = "1";
                return output;
            }

            string[] inputParts = input.ShortLobbyId.Split(SEPARATOR);
            if (inputParts.Length != 2)
            {
                output.FailReason = "2";
                return output;
            }

            int hoodId;
            if (!int.TryParse(inputParts[0], out hoodId))
            {
                output.FailReason = "3";
                return output;
            }

            PrivateLobbyIdMapItemHelper helper = new PrivateLobbyIdMapItemHelper(new AmazonDynamoDBClient(), input.LogsEnabled, context);
            PrivateLobbyIdMapItem? item = await helper.GetPrivateLobbyIdMapItem(hoodId);

            if (item == null)
            {
                output.FailReason = "4";
                return output;
            }

            if (input.LogsEnabled)
                context.Logger.Log("[HOOD][LAMBDA] - found entry");

            string[] itemParts = item.ShortPrivateLobbyId.Split(SEPARATOR);
            if (itemParts.Length != 2)
            {
                output.FailReason = "5";
                return output;
            }

            if (itemParts[1] != inputParts[1])
            {
                output.FailReason = "6 " + itemParts[1] + " " + inputParts[1];
                return output;
            }

            output.GamesSessionId = item.LongPrivateLobbyId;
            output.Success = true;
            return output;
        }
    }
}
