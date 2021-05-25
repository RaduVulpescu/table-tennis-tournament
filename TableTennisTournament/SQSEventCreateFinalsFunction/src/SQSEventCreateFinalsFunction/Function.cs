using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SQSEventCreateFinalsFunction
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

        public async Task FunctionHandler(SQSEvent ev, ILambdaContext context)
        {
            foreach (var sqsMessage in ev.Records)
            {
                context.Logger.LogLine($"Processing message to create the finals: {sqsMessage.Body}.");
                await ProcessMessageAsync(sqsMessage, context);
                context.Logger.LogLine("Finished processing message to create all finals.");
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            if (!TryDeserializeBody<Season>(message.Body, out var newCreatedSeason, out var error))
            {
                context.Logger.LogLine(error);
                return;
            }

            var beginnersFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.BeginnerFinal, 0);
            var intermediateFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.IntermediateFinal, 0);
            var advancedFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.AdvancedFinal, 0);
            var openFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.OpenFinal, 0);

            var createFinalsTasks = new[]
            {
                _seasonRepository.SaveAsync(beginnersFinal),
                _seasonRepository.SaveAsync(intermediateFinal),
                _seasonRepository.SaveAsync(advancedFinal),
                _seasonRepository.SaveAsync(openFinal)
            };

            await Task.WhenAll(createFinalsTasks);
        }
    }
}
