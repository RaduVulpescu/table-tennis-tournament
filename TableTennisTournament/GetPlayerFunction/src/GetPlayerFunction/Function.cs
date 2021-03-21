using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPlayerFunction
{
    public class Function : DynamoFunction
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerId = request.PathParameters["playerId"];

            var player = await DbContext.LoadAsync<Player>($"PLAYER#{playerId}", $"PLAYERDATA#{playerId}");

            return player is null
                ? new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Player {playerId} Not Found",
                    StatusCode = 404
                }
                : new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonConvert.SerializeObject(player),
                    StatusCode = 200
                };
        }
    }
}
