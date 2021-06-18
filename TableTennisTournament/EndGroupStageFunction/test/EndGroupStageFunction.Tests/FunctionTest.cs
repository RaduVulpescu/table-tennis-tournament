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
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 4) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player1Guid && fpr.Rank == 1 && fpr.Score == 78.24) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player2Guid && fpr.Rank == 2 && fpr.Score == 76.24) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player3Guid && fpr.Rank == 3 && fpr.Score == 75.24) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player4Guid && fpr.Rank == 4 && fpr.Score == 74.24)
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
                f.Players.Any(fp => fp.PlayerId == TestData.Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player4Guid && fp.GroupRank == 5) &&
                f.Players.Any(fp => fp.PlayerId == TestData.Player5Guid && fp.GroupRank == 4) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player1Guid && fpr.Rank == 1 && fpr.Score == 71.86) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player2Guid && fpr.Rank == 2 && fpr.Score == 69.86) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player3Guid && fpr.Rank == 3 && fpr.Score == 68.86) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player4Guid && fpr.Rank == 5 && fpr.Score == 66.86) &&
                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player5Guid && fpr.Rank == 4 && fpr.Score == 67.86)

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

                f.Pyramids.Any(p =>
                    p.Type == PyramidType.Ranks_1_2 &&
                    p.FindMatchByPlayers(TestData.Player1Guid, TestData.Player6Guid).Level == 0) &&

                f.Pyramids.Any(p =>
                    p.Type == PyramidType.Ranks_3_4 &&
                    p.FindMatchByPlayers(TestData.Player2Guid, TestData.Player7Guid).Level == 0) &&

                f.Pyramids.Any(p =>
                    p.Type == PyramidType.Ranks_5_6 &&
                    p.FindMatchByPlayers(TestData.Player3Guid, TestData.Player8Guid).Level == 0) &&

                f.Pyramids.Any(p =>
                    p.Type == PyramidType.Ranks_7_8 &&
                    p.FindMatchByPlayers(TestData.Player5Guid, TestData.Player9Guid).Level == 0) &&

                f.Ranking.Any(fpr => fpr.PlayerId == TestData.Player4Guid && fpr.Rank == 9 && fpr.Score == 71.24)

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

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 &&
                    p.FindMatchByPlayers(TestData.Player1Guid, TestData.Player14Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 &&
                    p.FindMatchByPlayers(TestData.Player9Guid, TestData.Player6Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 &&
                    p.FindMatchByPlayers(TestData.Player5Guid, TestData.Player10Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 &&
                    p.FindMatchByPlayers(TestData.Player13Guid, TestData.Player2Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_9_10 &&
                    p.FindMatchByPlayers(TestData.Player3Guid, TestData.Player16Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_9_10 &&
                    p.FindMatchByPlayers(TestData.Player11Guid, TestData.Player8Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_9_10 &&
                    p.FindMatchByPlayers(TestData.Player7Guid, TestData.Player12Guid).Level == 2) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_9_10 &&
                    p.FindMatchByPlayers(TestData.Player15Guid, TestData.Player4Guid).Level == 2)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }
    }
}
