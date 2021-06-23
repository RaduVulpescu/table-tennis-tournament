using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Players.Repository;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace EndFixtureFunction
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

            if (fixture.Players.Count != fixture.Ranking.Count)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Fixture is not finished",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            var seasonPlayers = await _seasonRepository.ListSeasonPlayersAsync(seasonId);

            foreach (var rank in fixture.Ranking)
            {
                var player = fixture.Players.Single(p => p.PlayerId == rank.PlayerId);
                var playerData = await _playerRepository.LoadAsync(Player.CreatePK(player.PlayerId.ToString()),
                    Player.CreateSK(player.PlayerId.ToString()));

                if (seasonPlayers.All(sp => sp.PlayerId != rank.PlayerId))
                {
                    player.Quality ??= rank.Score;

                    context.Logger.LogLine($"player: {JsonConvert.SerializeObject(player)}");
                    context.Logger.LogLine($"rank: {JsonConvert.SerializeObject(rank)}");
                    context.Logger.LogLine($"playerData: {JsonConvert.SerializeObject(playerData)}");

                    var seasonPlayer = SeasonPlayer.Create(seasonId, player.PlayerId.ToString(),
                        rank.PlayerName, playerData.CurrentLevel, player.Quality.Value, rank.Score);

                    seasonPlayers.Add(seasonPlayer);
                }
                else
                {
                    var seasonPlayer = seasonPlayers.Single(sp => sp.PlayerId == rank.PlayerId);

                    UpdateScores(rank, seasonPlayer);
                }
            }

            for (var i = 0; i < seasonPlayers.OrderBy(sp => sp.Top4).ToArray().Length; i++)
            {
                seasonPlayers[i].Rank = i + 1;
            }

            foreach (var seasonPlayer in seasonPlayers)
            {
                context.Logger.LogLine($"seasonPlayer: {JsonConvert.SerializeObject(seasonPlayer)}");
                await _seasonRepository.SaveAsync(seasonPlayer);
            }

            fixture.State = FixtureState.Finished;
            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }

        private static void UpdateScores(FixturePlayerRank rank, SeasonPlayer seasonPlayer)
        {
            if (rank.Score > seasonPlayer.Score1)
            {
                seasonPlayer.Score4 = seasonPlayer.Score3;
                seasonPlayer.Score3 = seasonPlayer.Score2;
                seasonPlayer.Score2 = seasonPlayer.Score1;
                seasonPlayer.Score1 = rank.Score;
            }
            else if (rank.Score > seasonPlayer.Score2)
            {
                seasonPlayer.Score4 = seasonPlayer.Score3;
                seasonPlayer.Score3 = seasonPlayer.Score2;
                seasonPlayer.Score2 = rank.Score;
            }
            else if (rank.Score > seasonPlayer.Score3)
            {
                seasonPlayer.Score4 = seasonPlayer.Score3;
                seasonPlayer.Score3 = rank.Score;
            }
            else if (rank.Score > seasonPlayer.Score4)
            {
                seasonPlayer.Score4 = rank.Score;
            }

            seasonPlayer.Top4 = seasonPlayer.Score1 + seasonPlayer.Score2 + seasonPlayer.Score3 + seasonPlayer.Score4;
        }
    }
}
