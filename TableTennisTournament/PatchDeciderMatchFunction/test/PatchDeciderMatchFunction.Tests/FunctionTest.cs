using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;
using Xunit;

namespace PatchDeciderMatchFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "FixtureWithZeroDepthPyramid"))
                .ReturnsAsync(CreateFixtureWithZeroDepthPyramid());

            _sutFunction = new Function(_seasonRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task EndGroupStageFunction_WithTwoGroups()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateTwoOrderedFourPlayersGroupFixture" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.DecidersStage &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        private SeasonFixture CreateFixtureWithZeroDepthPyramid()
        {
            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
            };
        }
    }
}
