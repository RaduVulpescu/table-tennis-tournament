using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SQSEventStartSeasonFunction
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
            foreach(var sqsMessage in ev.Records)
            {
                context.Logger.LogLine($"Processing message to start a new season: {sqsMessage.Body}.");
                if (!TryDeserializeBody<SNSEvent.SNSMessage>(sqsMessage.Body, out var snsMessage, out var snsDeserializationError))
                {
                    context.Logger.LogLine(snsDeserializationError);
                    return;
                }

                await ProcessMessageAsync(snsMessage, context);
            }
        }

        private async Task ProcessMessageAsync(SNSEvent.SNSMessage message, ILambdaContext context)
        {
            if (!TryDeserializeBody<Season>(message.Message, out var finishedSeason, out var seasonDeserializationError))
            {
                context.Logger.LogLine(seasonDeserializationError);
                return;
            }

            if (!finishedSeason.EndDate.HasValue)
            {
                context.Logger.LogLine($"Season with id {finishedSeason.SeasonId} does not have an endDate set.");
                return;
            }

            var newSeason = Season.Create(finishedSeason.Number + 1, finishedSeason.EndDate.Value.AddDays(1));

            await _seasonRepository.SaveAsync(newSeason);

            context.Logger.LogLine("Finished processing message to create a new season.");
        }
    }
}
