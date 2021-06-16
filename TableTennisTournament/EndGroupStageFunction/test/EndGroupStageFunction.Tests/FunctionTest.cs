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

namespace EndGroupStageFunction.Tests
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
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateOrderedFourPlayersGroupFixture"))
                .ReturnsAsync(TestData.CreateOrderedFourPlayersGroupFixture());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateTwoEqualVictoryPlayers"))
                .ReturnsAsync(TestData.CreateTwoEqualVictoryPlayers());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPartialBarrageFixtureWithATie"))
                .ReturnsAsync(TestData.CreateThreeManPartialBarrageFixtureWithATie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPartialBarrageFixtureWithNoTie"))
                .ReturnsAsync(TestData.CreateThreeManPartialBarrageFixtureWithNoTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManCompleteBarrageFixtureWithTie"))
                .ReturnsAsync(TestData.CreateThreeManCompleteBarrageFixtureWithTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManCompleteBarrageFixtureWithNoTie"))
                .ReturnsAsync(TestData.CreateThreeManCompleteBarrageFixtureWithNoTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPerfectTie"))
                .ReturnsAsync(TestData.CreateThreeManPerfectTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateTwoOrderedFourPlayersGroupFixture"))
                .ReturnsAsync(TestData.CreateTwoOrderedFourPlayersGroupFixture());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFourGroupsWithFourOrderedPlayersFixture"))
                .ReturnsAsync(TestData.CreateFourGroupsWithFourOrderedPlayersFixture());

            _seasonRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<SeasonFixture>()))
                .Returns(Task.CompletedTask);

            _sutFunction = new Function(_seasonRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task EndGroupStageFunction_WithPlayerWithDifferentNumberOfVictories_ShouldOrderThemFromHighestToLowest()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateOrderedFourPlayersGroupFixture" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player1Guid && f.Rank == 1 && f.Score == 78.24) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player2Guid && f.Rank == 2 && f.Score == 76.24) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player3Guid && f.Rank == 3 && f.Score == 75.24) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player4Guid && f.Rank == 4 && f.Score == 74.24)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithTwoPlayersHaveTheSameNumberOfVictories_ShouldPrioritizeDirectVictory()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateTwoEqualVictoryPlayers" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 5) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player5Guid && fp.GroupRank == 4) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player1Guid && f.Rank == 1 && f.Score == 71.86) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player2Guid && f.Rank == 2 && f.Score == 69.86) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player3Guid && f.Rank == 3 && f.Score == 68.86) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player4Guid && f.Rank == 5 && f.Score == 66.86) &&
                f.Ranking.Any(f => f.PlayerId == TestData.Player5Guid && f.Rank == 4 && f.Score == 67.86)

            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithThreePlayersWithTheSameNumberOfVictories_ShouldPrioritizeTheBestSetDifferenceBetweenThemAndDirectVictory()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateThreeManPartialBarrageFixtureWithATie" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithThreePlayersWithTheSameNumberOfVictories_ShouldPrioritizeTheBestSetDifferenceBetweenThem()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateThreeManPartialBarrageFixtureWithNoTie" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithThreePlayersWithTheSameNumberOfVictories_ShouldPrioritizeTheBestSetDifferenceBetweenAllGroupPlayersAndDirectVictory()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateThreeManCompleteBarrageFixtureWithTie" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 4) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 2)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithThreePlayersWithTheSameNumberOfVictories_ShouldPrioritizeTheBestSetDifferenceBetweenAllGroupPlayers()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateThreeManCompleteBarrageFixtureWithNoTie" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithThreePlayersAtPerfectTie_ShouldCoinFlipTheRanking()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateThreeManPerfectTie" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.Finished &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Single(p => p.PlayerId == TestData.Player2Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == TestData.Player3Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == TestData.Player4Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == TestData.Player2Guid).GroupRank != f.Players.Single(p => p.PlayerId == TestData.Player3Guid).GroupRank &&
                f.Players.Single(p => p.PlayerId == TestData.Player3Guid).GroupRank != f.Players.Single(p => p.PlayerId == TestData.Player4Guid).GroupRank &&
                f.Players.Single(p => p.PlayerId == TestData.Player4Guid).GroupRank != f.Players.Single(p => p.PlayerId == TestData.Player2Guid).GroupRank
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
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
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player5Guid && fp.GroupRank == 4) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 5) &&

                f.Players.Any(fp => fp.PlayerId == TestData.Player6Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player7Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player8Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player9Guid && fp.GroupRank == 4) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player1Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player6Guid &&
                    dm.Depth == 0 && dm.Pyramid == PyramidType.Ranks_1_2) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player2Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player7Guid &&
                    dm.Depth == 0 && dm.Pyramid == PyramidType.Ranks_3_4) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player3Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player8Guid &&
                    dm.Depth == 0 && dm.Pyramid == PyramidType.Ranks_5_6) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player5Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player9Guid &&
                    dm.Depth == 0 && dm.Pyramid == PyramidType.Ranks_7_8) &&

                f.Ranking.Any(f => f.PlayerId == TestData.Player4Guid && f.Rank == 9 && f.Score == 71.24)

            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task EndGroupStageFunction_WithFourGroups()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "seasonId", "" }, { "fixtureId", "CreateFourGroupsWithFourOrderedPlayersFixture" }
                }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.State == FixtureState.DecidersStage &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4) &&

                f.Players.Any(fp => fp.PlayerId == TestData.Player5Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player6Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player7Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player8Guid && fp.GroupRank == 4) &&

                f.Players.Any(fp => fp.PlayerId == TestData.Player9Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player10Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player11Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player12Guid && fp.GroupRank == 4) &&

                f.Players.Any(fp => fp.PlayerId == TestData.Player13Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player14Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player15Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player16Guid && fp.GroupRank == 4) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player1Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player14Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_1_2) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player9Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player6Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_1_2) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player5Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player10Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_1_2) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player13Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player2Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_1_2) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player3Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player16Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_9_10) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player11Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player8Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_9_10) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player7Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player12Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_9_10) &&

                f.DeciderMatches.Any(dm =>
                    dm.PlayerOneStats.PlayerId == TestData.Player15Guid &&
                    dm.PlayerTwoStats.PlayerId == TestData.Player4Guid &&
                    dm.Depth == 2 && dm.Pyramid == PyramidType.Ranks_9_10)

            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }
    }
}
