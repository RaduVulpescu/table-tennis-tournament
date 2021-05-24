using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using Xunit;

namespace PostPlayerFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<IPlayerRepository> _playerRepositoryMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _playerRepositoryMock = new Mock<IPlayerRepository>();

            _playerRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<Player>()))
                .Returns(Task.CompletedTask);

            _sutFunction = new Function(_playerRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task PostPlayerFunction_WithMalformedInput_ReturnsUnsupportedMediaType()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/malformed-player.json")
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _playerRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, actualResponse.StatusCode);
            Assert.Equal("Deserialization error: the field 'name' could not be deserialized.", actualResponse.Body);
        }

        [Fact]
        public async Task PostPlayerFunction_WithInvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = await File.ReadAllTextAsync("./json/invalid-player.json")
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _playerRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.BadRequest, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PostPlayerFunction_WithValidPlayer_ReturnsCreated()
        {
            // Arrange
            var playerJson = await File.ReadAllTextAsync("./json/player.json");
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = playerJson
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);
            var expectedPlayer = JsonConvert.DeserializeObject<Player>(playerJson);
            var actualPlayer = JsonConvert.DeserializeObject<Player>(actualResponse.Body);

            // Assert
            _playerRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Player>()), Times.Once);
            _playerRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.Created, actualResponse.StatusCode);

            Assert.StartsWith($"{Constants.PlayerPrefix}#", actualPlayer!.PK);
            Assert.StartsWith($"{Constants.PlayerDataPrefix}#", actualPlayer!.SK);
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
    }
}
