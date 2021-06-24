using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.DTO;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace PutFixtureFunction
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
            if (!TryDeserializeBody<FixturePutDTO>(request.Body, out var fixtureDTO, out var error))
            {
                context.Logger.Log(error);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            // TODO: implement validation for FixturePutDTO

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

            fixture.Update(
                fixtureDTO.Date.ToUniversalTime(),
                fixtureDTO.Location,
                fixtureDTO.Players
            );

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
