using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.Players.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace GetPlayersFunction
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
            var players = await _playerRepository.ListAsync();

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(players),
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
