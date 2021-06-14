using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.DTO;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace PatchGroupMatchFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
        }

        public Function(ISeasonRepository seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (!TryDeserializeBody<MatchPutDTO>(request.Body, out var matchPutDTO, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            var seasonId = request.PathParameters["seasonId"];
            var fixtureId = request.PathParameters["fixtureId"];
            var matchId = request.PathParameters["matchId"];

            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);
            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var match = fixture.GroupMatches.SingleOrDefault(gm => gm.MatchId.ToString() == matchId);
            if (match is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Match with id {matchId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            match.PlayerOneStats.SetsWon = matchPutDTO.SetsWonByPlayerOne;
            match.PlayerTwoStats.SetsWon = matchPutDTO.SetsWonByPlayerTwo;

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
