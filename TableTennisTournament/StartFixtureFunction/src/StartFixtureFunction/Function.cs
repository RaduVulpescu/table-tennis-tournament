using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StartFixtureFunction
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

            var seasonId = request.PathParameters["seasonId"];
            var fixtureId = request.PathParameters["fixtureId"];
            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);

            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            fixture.State = FixtureState.GroupsSelection;

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
