using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using Xunit;

namespace PostPlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly IDynamoDBContext _dbContext;

        public FunctionTest()
        {
            var dbContextMock = new Mock<IDynamoDBContext>();
            dbContextMock
                .Setup(x => x.SaveAsync(It.IsAny<Player>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            _dbContext = dbContextMock.Object;
        }

        [Fact]
        public async Task TestToUpperFunction()
        {
            // Arrange
            var function = new Function(_dbContext);
            var context = new TestLambdaContext();

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/player.json")
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal(201, actualResponse.StatusCode);
        }
    }
}
