using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PostPlayerFunction
{
    public class Function
    {
        public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playerDTO = JsonConvert.DeserializeObject<PlayerDTO>(request.Body);

            var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig());
            var dynamoDbContext = new DynamoDBContext(client, new DynamoDBContextConfig());

            var newPlayer = Player.Create(
                playerDTO.Name,
                playerDTO.BirthYear,
                playerDTO.City,
                playerDTO.CurrentLevel,
                playerDTO.Height,
                playerDTO.Weight
            );

            dynamoDbContext.SaveAsync(newPlayer).GetAwaiter().GetResult();

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "asd",
                Headers = new Dictionary<string, string>
                {
                    { "Location", $"~/players/{newPlayer.PK.Split('#')[1]}" }
                },
                StatusCode = 201
            };
        }
    }
}
