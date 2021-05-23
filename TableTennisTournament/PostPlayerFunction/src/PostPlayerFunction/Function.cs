using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Validators;
using TTT.Players.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace PostPlayerFunction
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
            if (!TryDeserializeBody<PlayerDTO>(request.Body, out var playerDTO, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
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

            var newPlayer = Player.Create(
                playerDTO.Name,
                playerDTO.City,
                playerDTO.BirthYear,
                playerDTO.Height,
                playerDTO.Weight,
                playerDTO.CurrentLevel
            );

            await _playerRepository.SaveAsync(newPlayer);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(newPlayer),
                Headers = new Dictionary<string, string>
                {
                    { "Location", $"~/players/{newPlayer.PK.Split('#')[1]}" }
                },
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
