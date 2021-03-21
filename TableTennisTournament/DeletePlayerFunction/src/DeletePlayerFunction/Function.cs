using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using TTT.DomainModel.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeletePlayerFunction
{
    public class Function : DynamoFunction
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerId = request.PathParameters["playerId"];

            var player = await DbContext.LoadAsync<Player>($"PLAYER#{playerId}", $"PLAYERDATA#{playerId}");

            if (player is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Player {playerId} Not Found",
                    StatusCode = 404
                };
            }

            await DbContext.DeleteAsync(player);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 204
            };
        }
    }
}
