using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Players.Repository;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace EndGroupStageFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly IPlayerRepository _playerRepository;

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
            _playerRepository = ServiceProvider.GetService<IPlayerRepository>();
        }

        public Function(ISeasonRepository seasonRepository, IPlayerRepository playerRepository)
        {
            _seasonRepository = seasonRepository;
            _playerRepository = playerRepository;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var seasonId = request.PathParameters["seasonId"];
            var fixtureId = request.PathParameters["fixtureId"];

            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);
            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            if (fixture.GroupMatches.Any(x => !x.PlayerOneStats.SetsWon.HasValue || !x.PlayerTwoStats.SetsWon.HasValue))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "All matches must be finished before ending the group stage."
                };
            }

            fixture.Ranking = new List<FixturePlayerRank>();

            var groups = fixture.GroupMatches.GroupBy(gm => gm.Group).Select(x => x.Key).ToArray();
            switch (groups.Length)
            {
                case 1:
                    HandleOneGroupEnding(fixture);
                    break;
                case 2:
                    HandleTwoGroupsEnding(fixture, groups);
                    break;
                case 4:
                    HandleFourGroupsEnding(fixture, groups);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        // ReSharper disable once PossibleLossOfFraction    
        private static void HandleOneGroupEnding(SeasonFixture fixture)
        {
            EndGroup(fixture.Players, fixture.GroupMatches);
            var orderedPlayers = fixture.Players.OrderByDescending(p => p.GroupRank).ToArray();

            var score = fixture.QualityAverage - (orderedPlayers.Length - 1) / 2;

            var i = 0;
            for (; i < orderedPlayers.Length - 1; i++)
            {
                fixture.Ranking.Add(new FixturePlayerRank
                {
                    PlayerId = orderedPlayers[i].PlayerId,
                    PlayerName = orderedPlayers[i].Name,
                    Rank = orderedPlayers[i].GroupRank!.Value,
                    Score = score
                });

                score += 1;
            }

            fixture.Ranking.Add(new FixturePlayerRank
            {
                PlayerId = orderedPlayers[i].PlayerId,
                PlayerName = orderedPlayers[i].Name,
                Rank = orderedPlayers[i].GroupRank!.Value,
                Score = score + 1
            });

            fixture.State = FixtureState.Finished;
        }

        private static void HandleTwoGroupsEnding(SeasonFixture fixture, IEnumerable<Group> groups)
        {
            foreach (var group in groups)
            {
                EndGroup(fixture.Players, fixture.GroupMatches.Where(gm => gm.Group == group).ToList());
            }

            CreateDecidersForTwoGroups(fixture);

            fixture.State = FixtureState.DecidersStage;
        }

        private static void HandleFourGroupsEnding(SeasonFixture fixture, IEnumerable<Group> groups)
        {
            foreach (var group in groups)
            {
                EndGroup(fixture.Players, fixture.GroupMatches.Where(gm => gm.Group == group).ToList());
            }

            CreateDecidersForFourGroups(fixture);

            fixture.State = FixtureState.DecidersStage;
        }

        private static void CreateDecidersForTwoGroups(SeasonFixture fixture)
        {
            fixture.DeciderMatches = new List<DeciderMatch>();

            var groupsAndPlayers = GetGroupsAndPlayers(fixture);

            var groupA = GetGroupPlayers(groupsAndPlayers, Group.A);
            var groupB = GetGroupPlayers(groupsAndPlayers, Group.B);

            var i = 0;
            for (; i < groupA.Length && i < groupB.Length; i++)
            {
                var groupAPlayer = groupA[0];
                var groupBPlayer = groupB[0];

                var matchId = Guid.NewGuid();

                fixture.DeciderMatches.Add(new DeciderMatch
                {
                    MatchId = matchId,
                    PlayerOneStats = new PlayerMatchStats
                    {
                        PlayerId = groupAPlayer.PlayerId,
                        PlayerName = groupAPlayer.Name
                    },
                    PlayerTwoStats = new PlayerMatchStats
                    {
                        PlayerId = groupBPlayer.PlayerId,
                        PlayerName = groupBPlayer.Name
                    }
                });
            }

            if (groupA.Length > groupB.Length)
            {
                fixture.Ranking.Add(new FixturePlayerRank
                {
                    PlayerId = groupA[i].PlayerId,
                    PlayerName = groupA[i].Name,
                    Rank = fixture.Players.Count,
                    Score = fixture.QualityAverage - (fixture.Players.Count - 1) / 2
                });
            }
            else if (groupA.Length < groupB.Length)
            {
                fixture.Ranking.Add(new FixturePlayerRank
                {
                    PlayerId = groupB[i].PlayerId,
                    PlayerName = groupB[i].Name,
                    Rank = fixture.Players.Count,
                    Score = fixture.QualityAverage - (fixture.Players.Count - 1) / 2
                });
            }
        }

        private static void CreateDecidersForFourGroups(SeasonFixture fixture)
        {
            fixture.DeciderMatches = new List<DeciderMatch>();

            var groupsAndPlayers = GetGroupsAndPlayers(fixture);

            var groupA = GetGroupPlayers(groupsAndPlayers, Group.A);
            var groupB = GetGroupPlayers(groupsAndPlayers, Group.B);
            var groupC = GetGroupPlayers(groupsAndPlayers, Group.C);
            var groupD = GetGroupPlayers(groupsAndPlayers, Group.D);

            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_1_2, 2, groupA[0], groupD[1]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_1_2, 2, groupC[0], groupB[1]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_1_2, 2, groupB[0], groupC[1]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_1_2, 2, groupD[0], groupA[1]));

            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_9_10, 2, groupA[2], groupD[3]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_9_10, 2, groupC[2], groupB[3]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_9_10, 2, groupB[2], groupC[3]));
            fixture.DeciderMatches.Add(DeciderMatch.Create(Guid.NewGuid(), PyramidType.Ranks_9_10, 2, groupD[2], groupA[3]));
        }

        private static List<Tuple<Group, List<FixturePlayer>>> GetGroupsAndPlayers(SeasonFixture fixture)
        {
            var groupsAndMatches = fixture.GroupMatches.GroupBy(x => x.Group);

            var groupsAndPlayers =
                (from @group in groupsAndMatches
                 let groupPlayers = fixture.Players.Where(fp =>
                     @group.Select(x => x.PlayerOneStats.PlayerId).Contains(fp.PlayerId) ||
                     @group.Select(x => x.PlayerTwoStats.PlayerId).Contains(fp.PlayerId)).ToList()
                 select new Tuple<Group, List<FixturePlayer>>(@group.Key, groupPlayers)).ToList();

            return groupsAndPlayers;
        }

        private static FixturePlayer[] GetGroupPlayers(IEnumerable<Tuple<Group, List<FixturePlayer>>> groupsAndPlayers, Group group)
        {
            return groupsAndPlayers
                .Single(gp => gp.Item1 == group).Item2
                .OrderBy(player => player.GroupRank)
                .ToArray();
        }

        private static void EndGroup(IEnumerable<FixturePlayer> fixturePlayers, IReadOnlyCollection<GroupMatch> groupMatches)
        {
            var groupPlayers = fixturePlayers.Where(fp => groupMatches.Any(gm =>
                gm.PlayerOneStats.PlayerId == fp.PlayerId || gm.PlayerTwoStats.PlayerId == fp.PlayerId));

            var victoryPerformances = groupPlayers
                    .Select(player => new PlayerPerformance(player, groupMatches.Count(IsWinner(player))))
                    .OrderByDescending(a => a.Factor).ToArray();

            var decidedRank = 1;

            for (var i = 0; i < victoryPerformances.Length; i++)
            {
                if (i == victoryPerformances.Length - 1) // if current player is last player
                {
                    victoryPerformances[i].FixturePlayer.GroupRank = decidedRank;
                    break;
                }

                var currentPlayer = victoryPerformances[i];
                var nextPlayer = victoryPerformances[i + 1];

                if (currentPlayer.Factor > nextPlayer.Factor) // if current player has more victories
                {
                    currentPlayer.FixturePlayer.GroupRank = decidedRank;
                    decidedRank++;
                    continue;
                }

                if (currentPlayer.Factor == nextPlayer.Factor &&             // if current player has the same number of victories as next player
                    (i + 1 == victoryPerformances.Length - 1 ||              // if nextPlayer is last player
                     nextPlayer.Factor > victoryPerformances[i + 2].Factor)) // if nextPlayer has more factor than player after him
                {
                    decidedRank = ResolveTieBetweenTwoPlayers(groupMatches, currentPlayer, nextPlayer, decidedRank);
                    i++;
                    continue;
                }

                var barragePlayers = new List<FixturePlayer> { currentPlayer.FixturePlayer };

                do
                {
                    barragePlayers.Add(victoryPerformances[i + 1].FixturePlayer);
                    i++;
                } while (i < victoryPerformances.Length - 1 && victoryPerformances[i].Factor == victoryPerformances[i + 1].Factor);

                decidedRank = ResolveBarrage(barragePlayers, groupMatches, decidedRank);
            }
        }

        private static int ResolveTieBetweenTwoPlayers(IEnumerable<GroupMatch> allGroupMatches, PlayerPerformance currentPlayer,
            PlayerPerformance nextPlayer, int decidedRank)
        {
            var directMatch = allGroupMatches.Single(IsDirectMatch(currentPlayer, nextPlayer));

            var currentPlayerIsTheWinner = IsWinner(currentPlayer.FixturePlayer).Invoke(directMatch);
            if (currentPlayerIsTheWinner)
            {
                currentPlayer.FixturePlayer.GroupRank = decidedRank;
                decidedRank++;
                nextPlayer.FixturePlayer.GroupRank = decidedRank;
                decidedRank++;
            }
            else
            {
                nextPlayer.FixturePlayer.GroupRank = decidedRank;
                decidedRank++;
                currentPlayer.FixturePlayer.GroupRank = decidedRank;
                decidedRank++;
            }

            return decidedRank;
        }

        private static Func<GroupMatch, bool> IsWinner(FixturePlayer player)
        {
            return gm =>
                gm.PlayerOneStats.PlayerId == player.PlayerId && gm.PlayerOneStats.SetsWon!.Value > gm.PlayerTwoStats.SetsWon!.Value ||
                gm.PlayerTwoStats.PlayerId == player.PlayerId && gm.PlayerOneStats.SetsWon!.Value < gm.PlayerTwoStats.SetsWon!.Value;
        }

        private static Func<GroupMatch, bool> IsDirectMatch(PlayerPerformance currentPlayer, PlayerPerformance nextPlayer)
        {
            return gm =>
                gm.PlayerOneStats.PlayerId == currentPlayer.FixturePlayer.PlayerId &&
                gm.PlayerTwoStats.PlayerId == nextPlayer.FixturePlayer.PlayerId ||
                gm.PlayerOneStats.PlayerId == nextPlayer.FixturePlayer.PlayerId &&
                gm.PlayerTwoStats.PlayerId == currentPlayer.FixturePlayer.PlayerId;
        }

        private static int ResolveBarrage(IReadOnlyCollection<FixturePlayer> partialBarragePlayers, IReadOnlyCollection<GroupMatch> allGroupMatches, int decidedRank)
        {
            var barragePlayersIds = partialBarragePlayers.Select(bp => bp.PlayerId).ToArray();

            var partialMatches = allGroupMatches.Where(gm =>
                barragePlayersIds.Contains(gm.PlayerOneStats.PlayerId) &&
                barragePlayersIds.Contains(gm.PlayerTwoStats.PlayerId)).ToArray();

            var playerToSetDifference = partialBarragePlayers
                .Select(player => new PlayerPerformance(player, GetSetsDifference(partialMatches, player)))
                .OrderByDescending(a => a.Factor).ToArray();

            for (var i = 0; i < playerToSetDifference.Length; i++)
            {
                if (i == playerToSetDifference.Length - 1) // if current player is last player
                {
                    playerToSetDifference[i].FixturePlayer.GroupRank = decidedRank;
                    break;
                }

                var currentPlayer = playerToSetDifference[i];
                var nextPlayer = playerToSetDifference[i + 1];

                if (currentPlayer.Factor > nextPlayer.Factor) // if the current player has a better set difference
                {
                    currentPlayer.FixturePlayer.GroupRank = decidedRank;
                    decidedRank++;
                    continue;
                }

                if (currentPlayer.Factor == nextPlayer.Factor &&               // if current player has the same set difference as next player
                    (i + 1 == playerToSetDifference.Length - 1 ||              // if nextPlayer is last player
                     nextPlayer.Factor > playerToSetDifference[i + 2].Factor)) // if nextPlayer has a better set difference than player after him
                {
                    decidedRank = ResolveTieBetweenTwoPlayers(allGroupMatches, currentPlayer, nextPlayer, decidedRank);
                    i++;
                    continue;
                }

                var completeBarragePlayers = new List<FixturePlayer> { playerToSetDifference[i].FixturePlayer };

                do
                {
                    completeBarragePlayers.Add(playerToSetDifference[i + 1].FixturePlayer);
                    i++;
                } while (i < playerToSetDifference.Length - 1 && playerToSetDifference[i].Factor == playerToSetDifference[i + 1].Factor);

                decidedRank = ResolveCompleteBarrage(completeBarragePlayers.ToArray(), allGroupMatches, decidedRank);
            }

            return decidedRank;
        }

        private static int GetSetsDifference(IEnumerable<GroupMatch> groupMatches, FixturePlayer player)
        {
            return groupMatches.Where(groupMatch =>
                    groupMatch.PlayerOneStats.PlayerId == player.PlayerId ||
                    groupMatch.PlayerTwoStats.PlayerId == player.PlayerId)
                .Sum(groupMatch => groupMatch.PlayerOneStats.PlayerId == player.PlayerId
                    ? groupMatch.PlayerOneStats.SetsWon!.Value - groupMatch.PlayerTwoStats.SetsWon!.Value
                    : groupMatch.PlayerTwoStats.SetsWon!.Value - groupMatch.PlayerOneStats.SetsWon!.Value);
        }

        private static int ResolveCompleteBarrage(IReadOnlyCollection<FixturePlayer> completeBarragePlayers,
            IReadOnlyCollection<GroupMatch> allGroupMatches, int decidedRank)
        {
            var playerToSetDifference = completeBarragePlayers
                .Select(player => new PlayerPerformance(player, GetSetsDifference(allGroupMatches, player)))
                .OrderByDescending(a => a.Factor).ToArray();

            for (var i = 0; i < playerToSetDifference.Length; i++)
            {
                if (i == playerToSetDifference.Length - 1) // if current player is last player
                {
                    playerToSetDifference[i].FixturePlayer.GroupRank = decidedRank;
                    break;
                }

                var currentPlayer = playerToSetDifference[i];
                var nextPlayer = playerToSetDifference[i + 1];

                if (currentPlayer.Factor > nextPlayer.Factor) // if the current player has a better set difference
                {
                    currentPlayer.FixturePlayer.GroupRank = decidedRank;
                    decidedRank++;
                    continue;
                }

                if (currentPlayer.Factor == nextPlayer.Factor &&               // if current player has the same set difference as next player
                    (i + 1 == playerToSetDifference.Length - 1 ||              // if nextPlayer is last player
                     nextPlayer.Factor > playerToSetDifference[i + 2].Factor)) // if nextPlayer has a better set difference than player after him
                {
                    decidedRank = ResolveTieBetweenTwoPlayers(allGroupMatches, currentPlayer, nextPlayer, decidedRank);
                    i++;
                    continue;
                }

                var random = new Random();
                var remainingRanks = Enumerable.Range(decidedRank, completeBarragePlayers.Count).ToList();
                foreach (var player in completeBarragePlayers)
                {
                    var coinFlipIndex = random.Next(remainingRanks.Count);
                    var coinFlipRank = remainingRanks[coinFlipIndex];
                    remainingRanks.RemoveAt(coinFlipIndex);

                    player.GroupRank = coinFlipRank;
                    decidedRank++;
                    i++;
                }

                i--; // current player is incremented one more time in the previous foreach loop
            }

            return decidedRank;
        }

        private readonly struct PlayerPerformance
        {
            public FixturePlayer FixturePlayer { get; }
            public int Factor { get; }

            public PlayerPerformance(FixturePlayer fixturePlayer, int factor)
            {
                FixturePlayer = fixturePlayer;
                Factor = factor;
            }
        }
    }
}
