using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel;
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

            var state = request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("state")
                ? request.QueryStringParameters["state"]
                : string.Empty;

            var dbFixtures = await _seasonRepository.LoadFixturesAsync(seasonId);
            var seasonFixtures = state == ((int) FixtureState.Upcoming).ToString()
                ? dbFixtures.Where(x => x.State == FixtureState.Upcoming).ToList()
                : dbFixtures.Where(x => x.State != FixtureState.Upcoming).ToList();

            var seasonFixturesDTO = seasonFixtures.Select(x => x.ToDTO());

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(seasonFixturesDTO),
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
