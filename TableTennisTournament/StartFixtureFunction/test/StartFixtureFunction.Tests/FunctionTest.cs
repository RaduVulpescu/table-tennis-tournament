using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Players.Repository;
using TTT.Seasons.Repository;
using Xunit;

namespace StartFixtureFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Mock<IPlayerRepository> _playerRepositoryMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        private const int Players8 = 8;

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();
            _playerRepositoryMock = new Mock<IPlayerRepository>();

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(Players8.ToString(), It.IsAny<string>()))
                .ReturnsAsync(CreateSeasonFixtureWithNPlayers(Players8));

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync("15", It.IsAny<string>()))
                .ReturnsAsync(CreateSeasonFixtureWithNPlayers(15));

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync("16", It.IsAny<string>()))
                .ReturnsAsync(CreateSeasonFixtureWithNPlayers(16));

            _seasonRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<SeasonFixture>()))
                .Returns(Task.CompletedTask);

            _sutFunction = new Function(_seasonRepositoryMock.Object, _playerRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task StartFixtureFunction_WithAn8PlayersFixture_ShouldCreate28Matches()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", Players8.ToString() }, { "fixtureId", "" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(Players8.ToString(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(p =>
                p.GroupMatches.Count == 28
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }

        [Fact]
        public async Task StartFixtureFunction_WithA15PlayersFixture_CreateMatches()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "15" }, { "fixtureId", "" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync("15", It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(p =>
                p.GroupMatches.Count == 49 &&
                p.GroupMatches.Count(gm => gm.Group == Group.A) == 21 &&
                p.GroupMatches.Count(gm => gm.Group == Group.B) == 28
            )), Times.Once);
            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }


        [Fact]
        public async Task StartFixtureFunction_WithA16PlayersFixture_CreateMatches()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "16" }, { "fixtureId", "" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync("16", It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(p =>
                p.GroupMatches.Count == 24 &&
                p.GroupMatches.Count(gm => gm.Group == Group.A) == 6 &&
                p.GroupMatches.Count(gm => gm.Group == Group.B) == 6 &&
                p.GroupMatches.Count(gm => gm.Group == Group.C) == 6 &&
                p.GroupMatches.Count(gm => gm.Group == Group.D) == 6
            )), Times.Once);
            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PostPlayerFunction_WithMalformedInput_ReturnsUnsupportedMediaType()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "8" }, { "fixtureId", "" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync("8", It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(p =>
                p.GroupMatches.Count == 28)
            ), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }

        private static SeasonFixture CreateSeasonFixtureWithNPlayers(int numberOfPlayers)
        {
            var season = new SeasonFixture
            {
                GroupMatches = new List<GroupMatch>(),
                Players = new List<FixturePlayer>()
            };

            const double qualityStart = 70d;

            for (var i = 0; i < numberOfPlayers; i++)
            {
                season.Players.Add(new FixturePlayer
                {
                    Name = $"Player {i + 1}",
                    Quality = qualityStart + i * 0.2
                });
            }

            return season;
        }
    }
}
