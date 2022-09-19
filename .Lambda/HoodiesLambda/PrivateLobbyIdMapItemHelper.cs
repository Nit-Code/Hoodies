using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;

namespace HoodiesLambda
{
    public class PrivateLobbyIdMapItemHelper
    {
        private IAmazonDynamoDB myDynamoDB;
        private ILambdaContext myLogContext;
        private bool myIsLogsEnabled;
        private const string TABLE_NAME = "PrivateLobbyIdMap";
        private readonly string[] COLUMN_NAMES = { "HoodId", "IsPrivateLobbyIdActive", "LongPrivateLobbyId", "PrivateLobbyIdCreationTime", "ShortPrivateLobbyId" };

        public async Task<PrivateLobbyIdMapItem?> GetPrivateLobbyIdMapItem(int aHoodId)
        {
            if (myIsLogsEnabled)
                myLogContext.Logger.Log("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem.");

            AttributeValue id = new AttributeValue();
            id.N = aHoodId.ToString();

            Dictionary<string, AttributeValue> key = GetItemKeyFromId(aHoodId.ToString());

            GetItemRequest getItemRequest = new GetItemRequest(TABLE_NAME, key);
            GetItemResponse getItemResponse = await myDynamoDB.GetItemAsync(getItemRequest);

            if (getItemResponse == null) 
            {
                if(myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 1. getItemResponse == null");
                return null;
            }

            if (getItemResponse.Item == null)
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 2. getItemResponse.Item == null");
                return null;
            }

            int found = 0;

            PrivateLobbyIdMapItem newItem = new PrivateLobbyIdMapItem();
            if (getItemResponse.Item.TryGetValue(COLUMN_NAMES[0], out AttributeValue? hoodId))
            {
                newItem.HoodId = Int32.Parse(hoodId.N);
                found++;
            }
            else 
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 3.");
            }

            if (getItemResponse.Item.TryGetValue(COLUMN_NAMES[1], out AttributeValue? isPrivateLobbyIdActive))
            {
                newItem.IsPrivateLobbyIdActive = isPrivateLobbyIdActive.BOOL;
                found++;
            }
            else
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 4.");
            }


            if (getItemResponse.Item.TryGetValue(COLUMN_NAMES[2], out AttributeValue? longPrivateLobbyId))
            {
                newItem.LongPrivateLobbyId = longPrivateLobbyId.S;
                found++;
            }
            else
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 5.");
            }


            if (getItemResponse.Item.TryGetValue(COLUMN_NAMES[3], out AttributeValue? privateLobbyIdCreationTime))
            {
                newItem.PrivateLobbyIdCreationTime = privateLobbyIdCreationTime.S;
                found++;
            }
            else
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 6.");
            }

            if (getItemResponse.Item.TryGetValue(COLUMN_NAMES[4], out AttributeValue? shortPrivateLobbyId))
            {
                newItem.ShortPrivateLobbyId = shortPrivateLobbyId.S;
                found++;
            }
            else
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - GetPrivateLobbyIdMapItem. Fail 7.");
            }

            if (found == COLUMN_NAMES.Length)
            {
                return newItem;
            }

            return null;
        }

        private Dictionary<string, AttributeValue> GetItemKeyFromId(string anId)
        {
            Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>();
            if (string.IsNullOrEmpty(anId))
            {
                return key;
            }

            AttributeValue id = new AttributeValue();
            id.N = anId;
            key.Add(COLUMN_NAMES[0], id);

            return key;
        }

        public async Task<bool> SetPrivateLobbyIdMapItem(PrivateLobbyIdMapItem anItem)
        {
            if (anItem == null || anItem.HoodId == -1)
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - SetPrivateLobbyIdMapItem - fail 1");
                return false;
            }

            Dictionary<string, AttributeValue> key = GetItemKeyFromId(anItem.HoodId.ToString());
            if (key.Count == 0)
            {
                if (myIsLogsEnabled)
                    myLogContext.Logger.LogError("[HOOD][LAMBDA] - SetPrivateLobbyIdMapItem - fail 2");
                return false;
            }

            UpdateItemRequest updateRequest = new UpdateItemRequest
            {
                TableName = TABLE_NAME,
                Key = key,
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    { "#name1", COLUMN_NAMES[1] },
                    { "#name2", COLUMN_NAMES[2] },
                    { "#name3", COLUMN_NAMES[3] },
                    { "#name4", COLUMN_NAMES[4] }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    { ":val1", new AttributeValue { BOOL = anItem.IsPrivateLobbyIdActive } },
                    { ":val2", new AttributeValue { S = anItem.LongPrivateLobbyId } },
                    { ":val3", new AttributeValue { S = anItem.PrivateLobbyIdCreationTime } },
                    { ":val4", new AttributeValue { S = anItem.ShortPrivateLobbyId } }
                },
                UpdateExpression = "SET #name1 = :val1, #name2 = :val2, #name3 = :val3, #name4 = :val4"
            };

            UpdateItemResponse updateResponse = await myDynamoDB.UpdateItemAsync(updateRequest);
            return updateResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public PrivateLobbyIdMapItemHelper(IAmazonDynamoDB aDynamoDB, bool anIsLogsEnabled, ILambdaContext aContext)
        {
            myDynamoDB = aDynamoDB;
            myIsLogsEnabled = anIsLogsEnabled;
            myLogContext = aContext;
        }
    }
}
