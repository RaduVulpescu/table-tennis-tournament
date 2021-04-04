using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.Players.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeletePlayerFunction
{
    public class Function : BaseFunction
    {
        private readonly IPlayerRepository _playerRepository;

        public Function()
        {
            _playerRepository = ServiceProvider.GetService<IPlayerRepository>();
        }

        public Function(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerId = request.PathParameters["playerId"];

            var player = await _playerRepository.LoadAsync($"PLAYER#{playerId}", $"PLAYERDATA#{playerId}");
            if (player is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Player {playerId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            await _playerRepository.DeleteAsync(player);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
