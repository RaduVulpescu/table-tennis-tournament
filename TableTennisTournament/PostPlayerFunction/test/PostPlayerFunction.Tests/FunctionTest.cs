using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace PostPlayerFunction.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {
            // Arrange
            var function = new Function();
            var context = new TestLambdaContext();

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = "{\"name\":\"Radu Vulpescu\",\"city\":\"Vaslui\",\"height\":\"180\",\"weight\":\"70\",\"currentLevel\":\"3\"}"
            };

            // Act
            var response = function.FunctionHandler(request, context);

            // Assert
            Assert.Equal(201, response.GetAwaiter().GetResult().StatusCode);
        }
    }
}
