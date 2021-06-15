using System;
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

namespace EndGroupStageFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Mock<IPlayerRepository> _playerRepositoryMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();
            _playerRepositoryMock = new Mock<IPlayerRepository>();

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateOrderedFourPlayersGroupFixture"))
                .ReturnsAsync(CreateOrderedFourPlayersGroupFixture());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateTwoEqualVictoryPlayers"))
                .ReturnsAsync(CreateTwoEqualVictoryPlayers());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPartialBarrageFixtureWithATie"))
                .ReturnsAsync(CreateThreeManPartialBarrageFixtureWithATie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPartialBarrageFixtureWithNoTie"))
                .ReturnsAsync(CreateThreeManPartialBarrageFixtureWithNoTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManCompleteBarrageFixtureWithTie"))
                .ReturnsAsync(CreateThreeManCompleteBarrageFixtureWithTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManCompleteBarrageFixtureWithNoTie"))
                .ReturnsAsync(CreateThreeManCompleteBarrageFixtureWithNoTie());

            _seasonRepositoryMock
                .Setup(x => x.LoadFixtureAsync(It.IsAny<string>(), "CreateThreeManPerfectTie"))
                .ReturnsAsync(CreateThreeManPerfectTie());

            _seasonRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<SeasonFixture>()))
                .Returns(Task.CompletedTask);

            _sutFunction = new Function(_seasonRepositoryMock.Object, _playerRepositoryMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task TBA_1()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task TBA_2()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 5) &&
                f.Players.Any(fp => fp.PlayerId == Player5Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task TBA_3()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task TBA_4()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }


        [Fact]
        public async Task TBA_5()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 4) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 2)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task TBA_6()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Any(fp => fp.PlayerId == Player2Guid && fp.GroupRank == 3) &&
                f.Players.Any(fp => fp.PlayerId == Player3Guid && fp.GroupRank == 2) &&
                f.Players.Any(fp => fp.PlayerId == Player4Guid && fp.GroupRank == 4)
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }

        [Fact]
        public async Task TBA_7()
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
                f.Players.Any(fp => fp.PlayerId == Player1Guid && fp.GroupRank == 1) &&
                f.Players.Single(p => p.PlayerId == Player2Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == Player3Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == Player4Guid).GroupRank.HasValue &&
                f.Players.Single(p => p.PlayerId == Player2Guid).GroupRank != f.Players.Single(p => p.PlayerId == Player3Guid).GroupRank &&
                f.Players.Single(p => p.PlayerId == Player3Guid).GroupRank != f.Players.Single(p => p.PlayerId == Player4Guid).GroupRank &&
                f.Players.Single(p => p.PlayerId == Player4Guid).GroupRank != f.Players.Single(p => p.PlayerId == Player2Guid).GroupRank
            )), Times.Once);

            _seasonRepositoryMock.VerifyNoOtherCalls();

            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
        }


        private static readonly Guid Player1Guid = Guid.Parse("E738582B-74CB-46B7-9EAB-6ADDEA6E48AC");
        private static readonly Guid Player2Guid = Guid.Parse("B0FB4E6D-A39D-436B-9F0A-AEB703681998");
        private static readonly Guid Player3Guid = Guid.Parse("E6FCD455-EAE1-44BB-9552-202F147AAE4F");
        private static readonly Guid Player4Guid = Guid.Parse("16B42531-16EB-48E9-A709-E4A63F699007");
        private static readonly Guid Player5Guid = Guid.Parse("23292D00-4C56-4558-8F19-7F1273A5DAC4");
        private static readonly Guid Player6Guid = Guid.Parse("936B6F49-D438-4F74-ADB6-8A5F1009D5FD");

        private static SeasonFixture CreateOrderedFourPlayersGroupFixture()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0),
                }
            };
        }

        private static SeasonFixture CreateTwoEqualVictoryPlayers()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player5Guid, 3, 0),


                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player3Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player4Guid, Player5Guid, 0, 3)
                }
            };
        }

        private static SeasonFixture CreateThreeManPartialBarrageFixtureWithATie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3),
                }
            };
        }

        private static SeasonFixture CreateThreeManPartialBarrageFixtureWithNoTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 1, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3),
                }
            };
        }

        private static SeasonFixture CreateThreeManCompleteBarrageFixtureWithTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 1),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 2, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 2),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3),
                }
            };
        }

        private static SeasonFixture CreateThreeManCompleteBarrageFixtureWithNoTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 1),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 2),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 2, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 2),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3),
                }
            };
        }

        private static SeasonFixture CreateThreeManPerfectTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 0, 3),
                }
            };
        }

        private static GroupMatch CreateGroupMatch(Group group, Guid playerOneGuid, Guid playerTwoGuid, int playerOneScore, int playerTwoScore) => new GroupMatch
        {
            Group = group,
            PlayerOneStats = new PlayerMatchStats { PlayerId = playerOneGuid, SetsWon = playerOneScore },
            PlayerTwoStats = new PlayerMatchStats { PlayerId = playerTwoGuid, SetsWon = playerTwoScore }
        };
    }
}
