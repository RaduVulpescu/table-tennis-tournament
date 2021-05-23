using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using Xunit;

namespace PutPlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly IPlayerRepository _playerRepository;

        private const string ExistingPlayer = "4b2e2992-0dec-40ee-ac89-e2d5d35d363b";

        public FunctionTest()
        {
            var playerRepositoryMock = new Mock<IPlayerRepository>();

            playerRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<Player>()))
                .Returns(Task.CompletedTask);

            playerRepositoryMock
                .Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Player)null);

            playerRepositoryMock
                .Setup(x => x.LoadAsync($"PLAYER#{ExistingPlayer}", $"PLAYERDATA#{ExistingPlayer}"))
                .ReturnsAsync(new Player());

            _playerRepository = playerRepositoryMock.Object;
        }

        [Fact]
        public async Task PutPlayerFunction_WithMalformedInput_ReturnsBadRequest()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/malformed-player.json")
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, actualResponse.StatusCode);
            Assert.Equal("Deserialization error: the field 'name' could not be deserialized.", actualResponse.Body);
        }

        [Fact]
        public async Task PutPlayerFunction_WithInvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/invalid-player.json"),
                PathParameters = new Dictionary<string, string> { { "playerId", ExistingPlayer } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PutPlayerFunction_WithNonExistingPlayerId_ReturnsNotFound()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/player.json"),
                PathParameters = new Dictionary<string, string> { { "playerId", Guid.NewGuid().ToString() } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PutPlayerFunction_WithValidInput_ReturnsNoContent()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/player.json"),
                PathParameters = new Dictionary<string, string> { { "playerId", ExistingPlayer } }
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
