using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.Players.Repository;
using TTT.Seasons.Repository;
using TTT.Services;

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

        protected static bool TryDeserializeBody<T>(string body, out T @object, out string error)
        {
            error = default;

            try
            {
                @object = JsonConvert.DeserializeObject<T>(body);
            }
            catch (JsonReaderException e)
            {
                @object = default;
                error = $"Deserialization error: the field '{e.Path}' could not be deserialized.";
                return false;
            }

            return true;
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAWSService<IAmazonDynamoDB>();
            serviceCollection.AddScoped<IDynamoDBContext, DynamoDBContext>();

            serviceCollection.AddScoped<IPlayerRepository, PlayerRepository>();
            serviceCollection.AddScoped<ISeasonRepository, SeasonRepository>();

            serviceCollection.AddTransient<ISnsClient, SnsClient>();
        }
    }
}
