using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.Model;
using AWSLambdaInputOutput;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HoodiesLambda;

public class BasicFunction
{
    public BasicFunctionOutput BasicFunctionHandler(BasicFunctionInput input, ILambdaContext context)
    {
        BasicFunctionOutput output = new BasicFunctionOutput();
        output.Value = "Error";
        output.Success = false;

        if (input == null) 
            return output;

        if (input.LogsEnabled)
            context.Logger.Log("[HOOD][LAMBDA] - GetGameSessionIdHandler");

        output.Success = true;
        output.Value = input.Value;

        return output;
    }
}