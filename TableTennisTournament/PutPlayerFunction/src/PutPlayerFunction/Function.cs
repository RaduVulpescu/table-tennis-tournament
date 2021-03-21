using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Validators;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PutPlayerFunction
{
    public class Function : DynamoFunction
    {
        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerDTO = JsonConvert.DeserializeObject<PlayerDTO>(request.Body);
            var validationResult = await new PlayerValidator().ValidateAsync(playerDTO);
            if (!validationResult.IsValid)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonConvert.SerializeObject(validationResult.Errors),
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            var playerId = request.PathParameters["playerId"];

            var player = await DbContext.LoadAsync<Player>($"PLAYER#{playerId}", $"PLAYERDATA#{playerId}");

            if (player is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Player {playerId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            player.Update(
                playerDTO.Name,
                playerDTO.BirthYear,
                playerDTO.City,
                playerDTO.CurrentLevel,
                playerDTO.Height,
                playerDTO.Weight
            );

            await DbContext.SaveAsync(player);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
}
