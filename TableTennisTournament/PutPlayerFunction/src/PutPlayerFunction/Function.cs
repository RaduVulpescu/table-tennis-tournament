using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Validators;
using TTT.Players.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PutPlayerFunction
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
            if (!TryDeserializeBody<PlayerDTO>(request.Body, out var playerDTO, out var proxyErrorResponse))
            {
                return proxyErrorResponse;
            }

            var validationResult = await new PlayerValidator().ValidateAsync(playerDTO);
            if (!validationResult.IsValid)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonConvert.SerializeObject(validationResult.Errors),
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

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

            player.Update(
                playerDTO.Name,
                playerDTO.City,
                playerDTO.BirthYear,
                playerDTO.Height,
                playerDTO.Weight,
                playerDTO.CurrentLevel
            );

            await _playerRepository.SaveAsync(player);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
