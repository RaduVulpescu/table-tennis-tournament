using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace PatchDeciderMatchFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
        }

        public Function(ISeasonRepository seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (!TryDeserializeBody<MatchPutDTO>(request.Body, out var matchPutDTO, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            var seasonId = request.PathParameters["seasonId"];
            var fixtureId = request.PathParameters["fixtureId"];
            var matchIdString = request.PathParameters["matchId"];
            var exists = Guid.TryParse(matchIdString, out var matchId);

            if (!exists)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Match with id {matchIdString} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);
            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var pyramid = fixture.Pyramids.SingleOrDefault(a => a.FindMatchById(matchId) != null);
            if (pyramid is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Match with id {matchIdString} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var match = pyramid.FindMatchById(matchId);

            match.PlayerOneStats.SetsWon = matchPutDTO.SetsWonByPlayerOne;
            match.PlayerTwoStats.SetsWon = matchPutDTO.SetsWonByPlayerTwo;

            if (match.Level == 0)
            {
                DecideRanks(fixture, pyramid, match);
            }
            else
            {
                CreateNextMatches(fixture, pyramid, match);
            }

            await _seasonRepository.SaveAsync(fixture);

            var responseBody = new PatchedFixtureDTO
            {
                State = fixture.State,
                Pyramids = fixture.Pyramids.OrderBy(x => x.Type).Select(p => p.PyramidToDTO()),
                Ranking = fixture.Ranking
            };

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(responseBody)
            };
        }

        private static void DecideRanks(SeasonFixture fixture, Pyramid pyramid, Node match)
        {
            var (winner, loser) = FinishMatch(match);

            var matchPyramid = (int)pyramid.Type;
            var winnerRank = matchPyramid * 2 + 1;

            var averageRankingPyramid = fixture.Players.Count / 4;
            var supplement = averageRankingPyramid - matchPyramid;

            double winnerScore;
            double loserScore;

            var score = fixture.QualityAverage + supplement * 2;
            if (fixture.Players.Count % 2 == 0 && fixture.Players.Count % 4 != 0)
            {
                winnerScore = score + 1;
                loserScore = score;
            }
            else
            {
                winnerScore = score;
                loserScore = score - 1;
            }

            if (pyramid.Type == PyramidType.Ranks_1_2)
            {
                winnerScore++;
            }

            fixture.Ranking.Add(new FixturePlayerRank
            {
                PlayerId = winner.PlayerId,
                PlayerName = winner.PlayerName,
                Rank = winnerRank,
                Score = winnerScore
            });

            fixture.Ranking.Add(new FixturePlayerRank
            {
                PlayerId = loser.PlayerId,
                PlayerName = loser.PlayerName,
                Rank = winnerRank + 1,
                Score = loserScore
            });
        }

        private static void CreateNextMatches(SeasonFixture fixture, Pyramid pyramid, Node match)
        {
            var (winner, _) = FinishMatch(match);

            var parent = match.FindParent(pyramid);
            var sibling = match.IsLeft ? parent.Right : parent.Left;
            if (!sibling.IsFinished) return;

            parent.PlayerOneStats = new PlayerMatchStats
            {
                PlayerId = match.IsLeft ? winner.PlayerId : sibling.GetWinner().PlayerId,
                PlayerName = match.IsLeft ? winner.PlayerName : sibling.GetWinner().PlayerName
            };

            parent.PlayerTwoStats = new PlayerMatchStats
            {
                PlayerId = match.IsLeft ? sibling.GetWinner().PlayerId : winner.PlayerId,
                PlayerName = match.IsLeft ? sibling.GetWinner().PlayerName : winner.PlayerName
            };

            var newPyramidType = (PyramidType)(match.Level + (int)pyramid.Type);

            var levelThatContainsTheMatchesRequiredForTheNewPyramid = (int)newPyramidType;

            var matchesOnLevel = pyramid.FindMatchesOnLevel(levelThatContainsTheMatchesRequiredForTheNewPyramid);

            var allLevelMatchesAreFinished = matchesOnLevel.Aggregate(true,
                (current, matchOnLevel) => current && matchOnLevel.IsFinished);

            if (!allLevelMatchesAreFinished) return;

            AddNewPyramid(fixture, matchesOnLevel, newPyramidType);
        }

        private static void AddNewPyramid(SeasonFixture fixture, List<Node> matchesOnLevel, PyramidType newPyramidType)
        {
            var combatants = new List<Tuple<FixturePlayer, FixturePlayer>>();
            for (var i = 0; i < matchesOnLevel.Count - 1; i += 2)
            {
                var leftMatch = matchesOnLevel[i];
                var rightMatch = matchesOnLevel[i + 1];

                var firstCombatant = fixture.Players.Single(p => p.PlayerId == leftMatch.GetLoser().PlayerId);
                var secondCombatant = fixture.Players.Single(p => p.PlayerId == rightMatch.GetLoser().PlayerId);

                combatants.Add(new Tuple<FixturePlayer, FixturePlayer>(firstCombatant, secondCombatant));
            }

            var newPyramid = Pyramid.CreatePyramid(combatants, newPyramidType);

            fixture.Pyramids.Add(newPyramid);
        }

        private static Tuple<PlayerMatchStats, PlayerMatchStats> FinishMatch(Node match)
        {
            PlayerMatchStats winner;
            PlayerMatchStats loser;

            if (match.PlayerOneStats.SetsWon!.Value > match.PlayerTwoStats.SetsWon!.Value)
            {
                winner = match.PlayerOneStats;
                loser = match.PlayerTwoStats;
            }
            else
            {
                winner = match.PlayerTwoStats;
                loser = match.PlayerOneStats;
            }

            match.IsFinished = true;

            return new Tuple<PlayerMatchStats, PlayerMatchStats>(winner, loser);
        }
    }
}
