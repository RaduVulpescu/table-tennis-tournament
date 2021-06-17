using System;
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
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;
using Xunit;

namespace PatchDeciderMatchFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task PatchDeciderMatchFunction_WithMatch1_2MatchDepth0_ShouldAwardsRank1And2()
        {
            // Mock / SUT
            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthPyramid"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthPyramid());

            var sutFunction = new Function(_seasonRepositoryMock.Object);

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
            var actualResponse = await sutFunction.FunctionHandler(request, _testContext);

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
            // Mock / SUT
            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthPyramidForRank7And8"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthPyramidForRank7And8());

            var sutFunction = new Function(_seasonRepositoryMock.Object);

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
            var actualResponse = await sutFunction.FunctionHandler(request, _testContext);

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
            // Mock / SUT
            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWithZeroDepthMatchForRank5And6"))
                .ReturnsAsync(TestData.CreateFixtureWithZeroDepthMatchForRank5And6());

            var sutFunction = new Function(_seasonRepositoryMock.Object);

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
            var actualResponse = await sutFunction.FunctionHandler(request, _testContext);

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

        [Fact]
        public async Task PatchDeciderMatchFunction_With1_2PyramidAndAllLevel2MatchesFinished_ShouldUpdatePlayersForNextMatchAndCreateNextPyramid()
        {
            // Mock / SUT
            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateFixtureWith1_2Pyramid"))
                .ReturnsAsync(TestData.CreateFixtureWith1_2Pyramid());

            var sutFunction = new Function(_seasonRepositoryMock.Object);

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
                    { "fixtureId", "CreateFixtureWith1_2Pyramid" },
                    { "matchId", TestData.MatchGuid2.ToString() }
                }
            };

            // Act
            var actualResponse = await sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadFixtureAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _seasonRepositoryMock.Verify(x => x.SaveAsync(It.Is<SeasonFixture>(f =>
                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 && HasMatchById(p, TestData.MatchGuid2,
                    TestData.Player3Guid, TestData.Player4Guid, 3, 0)) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_1_2 && HasMatchById(p, TestData.MatchGuid5,
                    TestData.Player1Guid, TestData.Player3Guid, null, null)) &&

                f.Pyramids.Any(p => p.Type == PyramidType.Ranks_5_6 && HasMatchByPlayers(p,
                    TestData.Player2Guid, TestData.Player4Guid, null, null))
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        private static bool HasMatchById(Pyramid pyramid, Guid matchId, Guid playerOneId, Guid playerTwoId,
            int? playerOneScore, int? playerTwoScore)
        {
            var match = pyramid.FindMatchById(matchId);

            var shouldBeFinished = true;
            if (playerOneScore.HasValue && playerTwoScore.HasValue)
            {
                shouldBeFinished = match.IsFinished;
            }

            var hasGoodScores = match.PlayerOneStats.PlayerId == playerOneId && match.PlayerTwoStats.PlayerId == playerTwoId &&
                       match.PlayerOneStats.SetsWon == playerOneScore && match.PlayerTwoStats.SetsWon == playerTwoScore;

            return shouldBeFinished && hasGoodScores;
        }

        private static bool HasMatchByPlayers(Pyramid pyramid, Guid playerOneId, Guid playerTwoId, int? playerOneScore, int? playerTwoScore)
        {
            var match = pyramid.FindMatchByPlayers(playerOneId, playerTwoId);

            var shouldBeFinished = true;
            if (playerOneScore.HasValue && playerTwoScore.HasValue)
            {
                shouldBeFinished = match.IsFinished;
            }

            var hasGoodScores = match.PlayerOneStats.PlayerId == playerOneId && match.PlayerTwoStats.PlayerId == playerTwoId &&
                                match.PlayerOneStats.SetsWon == playerOneScore && match.PlayerTwoStats.SetsWon == playerTwoScore;

            return shouldBeFinished && hasGoodScores;
        }
    }
}
