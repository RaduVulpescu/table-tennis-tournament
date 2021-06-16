using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
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
            var matchId = request.PathParameters["matchId"];

            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);
            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            var match = fixture.DeciderMatches.SingleOrDefault(gm => gm.MatchId.ToString() == matchId);
            if (match is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Match with id {matchId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            match.PlayerOneStats.SetsWon = matchPutDTO.SetsWonByPlayerOne;
            match.PlayerTwoStats.SetsWon = matchPutDTO.SetsWonByPlayerTwo;

            if (match.Depth == 0)
            {
                DecideRanks(fixture, match);
            }
            else
            {
                HandleNextMatch(fixture, match);
            }

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }

        private static void DecideRanks(SeasonFixture fixture, DeciderMatch match)
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

            var matchPyramid = (int)match.Pyramid;
            var winnerRank = matchPyramid * 2 + 1;

            var averageRankingPyramid = fixture.Players.Count / 4;
            var supplement = matchPyramid - averageRankingPyramid;

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

        private void HandleNextMatch(SeasonFixture fixture, DeciderMatch match)
        {
            throw new System.NotImplementedException();
        }
    }
}
