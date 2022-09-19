using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using System.Text.Json.Serialization;
using System.Globalization;
using AWSLambdaInputOutput;

namespace HoodiesLambda
{
    public class CreateShortLobbyId
    {
        private const string DATE_PATTERN = "dd, MM, yyyy, hh:mm:ss tt"; // example: 14, 09, 2022, 06:08:05 AM
        private const string SEPARATOR = "_";
        private const int CHARACTERS_AMMOUNT = 4;
        
        private string ShortLobbyId(int aHoodId)
        {
            string suffix = RandomString(CHARACTERS_AMMOUNT);
            return aHoodId.ToString() + SEPARATOR + suffix;
        }

        private string RandomString(int aCharCount)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[aCharCount];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string finalString = new string(stringChars);
            return finalString;
        }

        private DateTime GetDateObject(string aSourceString)
        {
            DateTime result;
            if (DateTime.TryParseExact(aSourceString, DATE_PATTERN, null, DateTimeStyles.None, out result))
            {
                return result;
            }

            return DateTime.UnixEpoch;
        }

        private string GetDateString(DateTime aSourceObject)
        {
            return aSourceObject.ToString(DATE_PATTERN, CultureInfo.InvariantCulture);
        }

        public async Task<CreateShortLobbyIdOutput> CreateShortLobbyIdHandler(CreateShortLobbyIdInput input, ILambdaContext context)
        {
            context.Logger.Log("[HOOD][LAMBDA] - CreateShortLobbyId.");
            CreateShortLobbyIdOutput output = new CreateShortLobbyIdOutput();
            output.Success = false;
            output.ShortLobbyId = "";

            if (input == null)
            {
                context.Logger.LogError("[HOOD][LAMBDA] - CreateShortLobbyId. Fail 1. No input object.");
                return output;
            }
                
            if (string.IsNullOrEmpty(input.GamesSessionId))
            {
                if (input.LogsEnabled)
                    context.Logger.LogError("[HOOD][LAMBDA] - CreateShortLobbyId. Fail 2. No GamesSessionId");
                return output;
            }

            PrivateLobbyIdMapItemHelper helper = new PrivateLobbyIdMapItemHelper(new AmazonDynamoDBClient(), input.LogsEnabled, context);
            PrivateLobbyIdMapItem? item = await helper.GetPrivateLobbyIdMapItem(input.HoodId);

            if (item == null)
            {
                if(input.LogsEnabled)
                    context.Logger.LogError("[HOOD][LAMBDA] - CreateShortLobbyId. Fail 3. DB searched item is null.");
                return output;
            }

            string shortLobbyId = ShortLobbyId(input.HoodId);
            PrivateLobbyIdMapItem updatedItem = new PrivateLobbyIdMapItem();
            updatedItem.HoodId = item.HoodId; // NOTE: it is very important to use the id from the input here so we update the table entry where the request came from
            updatedItem.IsPrivateLobbyIdActive = true;
            updatedItem.LongPrivateLobbyId = input.GamesSessionId;
            updatedItem.PrivateLobbyIdCreationTime = GetDateString(DateTime.UtcNow);
            updatedItem.ShortPrivateLobbyId = shortLobbyId;

            bool result = await helper.SetPrivateLobbyIdMapItem(updatedItem);
            if (!result && input.LogsEnabled)
                context.Logger.LogError("[HOOD][LAMBDA] - CreateShortLobbyId. Fail 4. DB update failed.");

            output.Success = result;
            output.ShortLobbyId = shortLobbyId;

            return output;
        }
    }
}
