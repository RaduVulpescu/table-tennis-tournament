using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Players.Repository;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SQSEventUpdatePlayersStatsFunction
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

        public async Task FunctionHandler(SQSEvent ev, ILambdaContext context)
        {
            foreach (var sqsMessage in ev.Records)
            {
                context.Logger.LogLine($"Processing message to upload player stats: {sqsMessage.Body}.");
                if (!TryDeserializeBody<SNSEvent.SNSMessage>(sqsMessage.Body, out var snsMessage, out var snsDeserializationError))
                {
                    context.Logger.LogLine(snsDeserializationError);
                    continue;
                }

                await ProcessMessageAsync(snsMessage, context);

                context.Logger.LogLine("Finished processing message to upload player stats.");
            }
        }

        private async Task ProcessMessageAsync(SNSEvent.SNSMessage message, ILambdaContext context)
        {
            if (!TryDeserializeBody<Season>(message.Message, out var finishedSeason,
                out var seasonDeserializationError))
            {
                context.Logger.LogLine(seasonDeserializationError);
                return;
            }

            var dbPlayers = await _playerRepository.ListAsync();
            var fixtures = await _seasonRepository.LoadFixturesAsync(finishedSeason.SeasonId.ToString());

            var seasonRankingsByPlayer = fixtures
                .Where(x => x.Ranking != null && x.Ranking.Count > 0)
                .SelectMany(x => x.Ranking)
                .GroupBy(x => x.PlayerId);

            foreach (var playerRanking in seasonRankingsByPlayer)
            {
                var playerToUpdateStats = dbPlayers.Single(x => x.PlayerId == playerRanking.Key);

                var playerFirst4Scores = playerRanking.Select(x => x.Score).OrderByDescending(x => x).Take(4);
                var playerMaxRank = playerRanking.Max(x => x.Rank);
                var playerMaxScore = playerRanking.First().Score;
                var playerTop4 = playerFirst4Scores.Sum();

                if (playerMaxScore > playerToUpdateStats.BestScore.GetValueOrDefault(0)) playerToUpdateStats.BestScore = playerMaxScore;
                if (playerMaxRank > playerToUpdateStats.BestRanking.GetValueOrDefault(0)) playerToUpdateStats.BestRanking = playerMaxRank;
                if (playerTop4 > playerToUpdateStats.BestTop4.GetValueOrDefault(0d)) playerToUpdateStats.BestTop4 = playerTop4;

                playerToUpdateStats.BestLevel = playerToUpdateStats.CurrentLevel;

                playerToUpdateStats.Quality = playerRanking.Average(x => x.Score);

                UpdateSeasonsPlayed(playerToUpdateStats);
                
                await _playerRepository.SaveAsync(playerToUpdateStats);
            }
        }

        private static void UpdateSeasonsPlayed(Player player)
        {
            switch (player.CurrentLevel)
            {
                case Level.Beginner:
                    player.BeginnerSeasons++;
                    break;
                case Level.Intermediate:
                    player.IntermediateSeasons++;
                    break;
                case Level.Advanced:
                    player.AdvancedSeasons++;
                    break;
                case Level.Open:
                    player.OpenSeasons++;
                    break;
                default:
                    break;
            }
        }
    }
}
