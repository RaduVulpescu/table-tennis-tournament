using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace GetUpcomingFixturesFunction
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
            var season = await _seasonRepository.LoadSeasonAsync(seasonId);
            if (season is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Season with id {seasonId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var fixtures = await _seasonRepository.LoadFixturesAsync(Season.CreatePK(seasonId));
            var upcomingFixtures = fixtures.Where(x => x.State == FixtureState.Upcoming);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(upcomingFixtures),
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
