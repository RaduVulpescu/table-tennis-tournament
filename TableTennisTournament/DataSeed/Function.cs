using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace DataSeed
{
    public class Function : BaseFunction
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ISeasonRepository _seasonRepository;

        public Function()
        {
            _playerRepository = ServiceProvider.GetService<IPlayerRepository>();
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> CreateSeasonAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (!TryDeserializeBody<Season>(request.Body, out var season, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            await _seasonRepository.SaveAsync(season);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(season),
                Headers = new Dictionary<string, string>
                {
                    { "Location", $"~/seasons/{season.PK.Split('#')[1]}" }
                },
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        public async Task<int> CreatePlayerAsync(Player player)
        {
            await _playerRepository.SaveAsync(player);

            return (int)HttpStatusCode.Created;
        }

        public async Task<int> CreateSeasonPlayerAsync(SeasonPlayer seasonPlayer)
        {
            await _seasonRepository.SaveAsync(seasonPlayer);

            return (int)HttpStatusCode.Created;
        }
    }
}
