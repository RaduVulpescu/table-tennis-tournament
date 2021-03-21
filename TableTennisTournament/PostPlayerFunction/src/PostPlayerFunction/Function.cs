using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Validators;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PostPlayerFunction
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

            var newPlayer = Player.Create(
                playerDTO.Name,
                playerDTO.BirthYear,
                playerDTO.City,
                playerDTO.CurrentLevel,
                playerDTO.Height,
                playerDTO.Weight
            );

            await DbContext.SaveAsync(newPlayer);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(newPlayer),
                Headers = new Dictionary<string, string>
                {
                    { "Location", $"~/players/{newPlayer.PK.Split('#')[1]}" }
                },
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
