using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using Xunit;

namespace GetPlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly IPlayerRepository _playerRepository;

        private const string ExistingPlayer = "4b2e2992-0dec-40ee-ac89-e2d5d35d363b";

        public FunctionTest()
        {
            var playerRepositoryMock = new Mock<IPlayerRepository>();

            playerRepositoryMock
                .Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Player)null);

            playerRepositoryMock
                .Setup(x => x.LoadAsync($"PLAYER#{ExistingPlayer}", $"PLAYERDATA#{ExistingPlayer}"))
                .ReturnsAsync(new Player { Name = "Radu" });

            _playerRepository = playerRepositoryMock.Object;
        }

        [Fact]
        public async Task GetPlayerFunction_WithANonExistingPlayer_ReturnsNotFound()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { "playerId", Guid.NewGuid().ToString() } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, actualResponse.StatusCode);
        }

        [Fact]
        public async Task GetPlayerFunction_WithAnExistingPlayer_ReturnsOK()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { "playerId", ExistingPlayer } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
            Assert.Contains("Radu", actualResponse.Body);
        }

        private Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function(_playerRepository);
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }
    }
}
