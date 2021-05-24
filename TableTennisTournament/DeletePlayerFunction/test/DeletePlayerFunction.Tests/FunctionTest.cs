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

namespace DeletePlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly IPlayerRepository _playerRepository;

        private const string ExistingPlayerId = "4b2e2992-0dec-40ee-ac89-e2d5d35d363b";

        public FunctionTest()
        {
            var playerRepositoryMock = new Mock<IPlayerRepository>();

            playerRepositoryMock
                .Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Player)null);

            playerRepositoryMock
                .Setup(x => x.LoadAsync(Player.CreatePK(ExistingPlayerId), Player.CreateSK(ExistingPlayerId)))
                .ReturnsAsync(new Player());

            playerRepositoryMock
                .Setup(x => x.DeleteAsync(It.IsAny<Player>()))
                .Returns(Task.CompletedTask);

            _playerRepository = playerRepositoryMock.Object;
        }

        [Fact]
        public async Task DeletePlayerFunction_WithANonExistingPlayer_ReturnsNotFound()
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
        public async Task DeletePlayerFunction_WithAnExistingPlayer_ReturnsNoContent()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { "playerId", ExistingPlayerId } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }

        private Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function(_playerRepository);
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }
    }
}
