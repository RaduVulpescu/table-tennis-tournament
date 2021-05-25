using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService.Model;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Validators;
using TTT.Seasons.Repository;
using TTT.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace PatchEndSeasonFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ISnsClient _snsClient;

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
            _snsClient = ServiceProvider.GetService<ISnsClient>();
        }

        public Function(ISeasonRepository seasonRepository, ISnsClient snsClient)
        {
            _seasonRepository = seasonRepository;
            _snsClient = snsClient;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (!TryDeserializeBody<SeasonsPatchDTO>(request.Body, out var seasonDTO, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            var validationResult = await new SeasonValidator().ValidateAsync(seasonDTO);
            if (!validationResult.IsValid)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonConvert.SerializeObject(validationResult.Errors),
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            var seasonId = request.PathParameters["seasonId"];

            var currentSeason = await _seasonRepository.LoadSeasonAsync(seasonId);
            if (currentSeason is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Season {seasonId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            currentSeason.EndDate = seasonDTO.EndDate;
            await _seasonRepository.SaveAsync(currentSeason);

            await _snsClient.PublishAsync(new PublishRequest
            {
                Subject = $"Season {currentSeason.Number} has ended on {seasonDTO.EndDate}.",
                Message = JsonConvert.SerializeObject(currentSeason),
                TopicArn = "arn:aws:sns:eu-west-1:623072768925:endSeasonTopic"
            });

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
