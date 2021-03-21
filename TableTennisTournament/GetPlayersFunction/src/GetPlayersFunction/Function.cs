using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPlayersFunction
{
    public class Function : BaseFunction
    {
        private readonly IDynamoDBContext _dbContext;

        public Function()
        {
            _dbContext = ServiceProvider.GetService<IDynamoDBContext>();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var playersAsyncSearch = _dbContext.ScanAsync<Player>(new List<ScanCondition>
            {
                new ScanCondition("SK", ScanOperator.BeginsWith, "PLAYERDATA")
            });

            var players = await playersAsyncSearch.GetRemainingAsync();

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonConvert.SerializeObject(players),
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
