using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeletePlayerFunction
{
    public class Function : BaseFunction
    {
        private readonly IDynamoDBContext _dbContext;

        public Function()
        {
            _dbContext = ServiceProvider.GetService<IDynamoDBContext>();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerId = request.PathParameters["playerId"];

            var player = await _dbContext.LoadAsync<Player>($"PLAYER#{playerId}", $"PLAYERDATA#{playerId}");
            if (player is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Player {playerId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            await _dbContext.DeleteAsync(player);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
