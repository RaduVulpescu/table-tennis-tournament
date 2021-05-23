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
                await ProcessMessageAsync(sqsMessage, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message to create the finals: {message.Body}.");

            if (!TryDeserializeBody<Season>(message.Body, out var newCreatedSeason, out var error))
            {
                context.Logger.LogLine(error);
                return;
            }

            var beginnersFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.BeginnerFinal);
            var intermediateFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.IntermediateFinal);
            var advancedFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.AdvancedFinal);
            var openFinal = SeasonFixture.Create(newCreatedSeason.SeasonId, FixtureType.OpenFinal);

            var createFinalsTasks = new[]
            {
                _seasonRepository.SaveAsync(beginnersFinal),
                _seasonRepository.SaveAsync(intermediateFinal),
                _seasonRepository.SaveAsync(advancedFinal),
                _seasonRepository.SaveAsync(openFinal)
            };

            await Task.WhenAll(createFinalsTasks);

            context.Logger.LogLine("Finished processing message to create all finals.");
        }
    }
}
