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
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AddFixtureFunction
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
            if (!TryDeserializeBody<FixturePostDTO>(request.Body, out var fixtureDTO, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            // TODO: implement validation for FixturePostDTO

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

            var newFixture = SeasonFixture.Create(
                season.SeasonId,
                FixtureType.Normal,
                fixtureDTO.Date.ToUniversalTime(),
                fixtureDTO.Location
            );

            await _seasonRepository.SaveAsync(newFixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(newFixture),
                Headers = new Dictionary<string, string>
                {
                    { "Location", $"~/seasons/{seasonId}/fixtures/{newFixture.PK.Split('#')[1]}" }
                },
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
