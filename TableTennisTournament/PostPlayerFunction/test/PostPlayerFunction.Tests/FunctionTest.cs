using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using Xunit;

namespace PostPlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly IPlayerRepository _playerRepository;

        public FunctionTest()
        {
            var playerRepositoryMock = new Mock<IPlayerRepository>();

            playerRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<Player>()))
                .Returns(Task.CompletedTask);

            _playerRepository = playerRepositoryMock.Object;
        }

        [Fact]
        public async Task PostPlayerFunction_WithMalformedInput_ReturnsUnsupportedMediaType()
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
        public async Task PostPlayerFunction_WithInvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/invalid-player.json")
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PostPlayerFunction_WithValidPlayer_ReturnsCreated()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var playerJson = await File.ReadAllTextAsync("./json/player.json");
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = playerJson
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);
            var expectedPlayer = JsonConvert.DeserializeObject<Player>(playerJson);
            var actualPlayer = JsonConvert.DeserializeObject<Player>(actualResponse.Body);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, actualResponse.StatusCode);

            Assert.StartsWith("PLAYER#", actualPlayer!.PK);
            Assert.StartsWith("PLAYERDATA#", actualPlayer!.SK);
            Assert.NotEqual(Guid.Empty, actualPlayer!.PlayerId);

            Assert.Equal(expectedPlayer!.Name, actualPlayer!.Name);
            Assert.Equal(expectedPlayer!.City, actualPlayer!.City);
            Assert.Equal(expectedPlayer!.Height, actualPlayer!.Height);
            Assert.Equal(expectedPlayer!.Weight, actualPlayer!.Weight);
            Assert.Equal(expectedPlayer!.CurrentLevel, actualPlayer!.CurrentLevel);

            Assert.Null(actualPlayer!.Quality);
            Assert.Null(actualPlayer!.BestScore);
            Assert.Null(actualPlayer!.BestRanking);
            Assert.Null(actualPlayer!.BestTop4);
            Assert.Null(actualPlayer!.BestLevel);

            Assert.Equal(0, actualPlayer!.OpenCups);
            Assert.Equal(0, actualPlayer!.AdvancedCups);
            Assert.Equal(0, actualPlayer!.IntermediateCups);
            Assert.Equal(0, actualPlayer!.BeginnerCups);
            Assert.Equal(0, actualPlayer!.OpenSeasons);
            Assert.Equal(0, actualPlayer!.AdvancedSeasons);
            Assert.Equal(0, actualPlayer!.IntermediateSeasons);
            Assert.Equal(0, actualPlayer!.BeginnerSeasons);
        }

        private Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function(_playerRepository);
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }
    }
}
