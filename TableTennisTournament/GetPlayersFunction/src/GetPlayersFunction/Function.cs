using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPlayersFunction
{
    public class Function : DynamoFunction
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playersAsyncSearch = DbContext.ScanAsync<Player>(new List<ScanCondition>
            {
                new ScanCondition("SK", ScanOperator.BeginsWith, "PLAYERDATA")
            });

            var players = await playersAsyncSearch.GetRemainingAsync();

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(players),
                StatusCode = 200
            };
        }
    }
}
