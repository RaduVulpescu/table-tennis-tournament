using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
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
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthPyramid"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthPyramid());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthPyramidForRank7And8"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthPyramidForRank7And8());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthMatchForRank5And6"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthMatchForRank5And6());

            _sutFunction = new Function(_seasonRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task PatchDeciderMatchFunction_WithMatch1_2MatchDepth0_ShouldAwardsRank1And2()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(new MatchPutDTO
                {
                    SetsWonByPlayerOne = 3,
                    SetsWonByPlayerTwo = 0
                }),
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" },
                    { "fixtureId", "CreateFixtureWithZeroDepthPyramid" },
                    { "matchId", TestData.MatchGuid1.ToString() }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.Ranking.Count == 2 &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player1Guid && r.Rank == 1 && r.Score == 76) &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player2Guid && r.Rank == 2 && r.Score == 74)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }


        [Fact]
        public async Task PatchDeciderMatchFunction_WithMatch7_8MatchDepth0_ShouldAwardsRank7And8()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(new MatchPutDTO
                {
                    SetsWonByPlayerOne = 0,
                    SetsWonByPlayerTwo = 3
                }),
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" },
                    { "fixtureId", "CreateFixtureWithZeroDepthPyramidForRank7And8" },
                    { "matchId", TestData.MatchGuid1.ToString() }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.Ranking.Count == 2 &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player2Guid && r.Rank == 7 && r.Score == 68) &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player1Guid && r.Rank == 8 && r.Score == 67)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PatchDeciderMatchFunction_WithPyramid5_6MatchDepth0_ShouldAwardsRank5And6()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(new MatchPutDTO
                {
                    SetsWonByPlayerOne = 3,
                    SetsWonByPlayerTwo = 0
                }),
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" },
                    { "fixtureId", "CreateFixtureWithZeroDepthMatchForRank5And6" },
                    { "matchId", TestData.MatchGuid1.ToString() }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.Ranking.Count == 2 &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player1Guid && r.Rank == 5 && r.Score == 70) &&
                f.Ranking.Any(r => r.PlayerId == TestData.Player2Guid && r.Rank == 6 && r.Score == 69)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }
    }
}
