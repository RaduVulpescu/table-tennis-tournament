using System;
using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.Players.Repository;

namespace FunctionCommon
{
    public abstract class BaseFunction
    {
        protected IServiceProvider ServiceProvider { get; set; }

        protected BaseFunction()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        protected static bool TryDeserializeBody<T>(string body, out T @object, out APIGatewayHttpApiV2ProxyResponse proxyErrorResponse)
        {
            try
            {
                @object = JsonConvert.DeserializeObject<T>(body);
            }
            catch (JsonReaderException e)
            {
                @object = default;
                proxyErrorResponse = new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"The field '{e.Path}' could not be deserialized.",
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };

                return false;
            }

            proxyErrorResponse = null;

            return true;
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAWSService<IAmazonDynamoDB>();
            serviceCollection.AddScoped<IDynamoDBContext, DynamoDBContext>();

            serviceCollection.AddScoped<IPlayerRepository, PlayerRepository>();
        }
    }
}
