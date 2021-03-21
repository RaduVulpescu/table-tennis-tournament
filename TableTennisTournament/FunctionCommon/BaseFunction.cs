using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;

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

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAWSService<IAmazonDynamoDB>();
            serviceCollection.AddScoped<IDynamoDBContext, DynamoDBContext>();
        }
    }
}
