using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TTT.DomainModel.Entities;
using Xunit;

namespace DataSeed.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task Seed_Season()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var season = Season.Create(1, DateTime.Now);
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(season)
            };

            // Act
            var actualResponse = await function.CreateSeasonAsync(request, context);

            // Assert
            Assert.Equal((int) HttpStatusCode.Created, actualResponse.StatusCode);
        }

        private static Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }
    }
}
