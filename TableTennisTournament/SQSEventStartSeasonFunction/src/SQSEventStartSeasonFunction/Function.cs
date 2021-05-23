using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.Seasons.Repository;
using TTT.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SQSEventStartSeasonFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ISqsClient _sqsClient;

        private const string CreateFinalQueueUrl =
            "https://sqs.eu-west-1.amazonaws.com/623072768925/TableTennisTournamentStack-CreateFinalsQueueA094EEE3-1SLFTOGL720H9";

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
            _sqsClient = ServiceProvider.GetService<ISqsClient>();
        }

        public Function(ISeasonRepository seasonRepository, ISqsClient sqsClient)
        {
            _seasonRepository = seasonRepository;
            _sqsClient = sqsClient;
        }

        public async Task FunctionHandler(SQSEvent ev, ILambdaContext context)
        {
            foreach(var sqsMessage in ev.Records)
            {
                context.Logger.LogLine($"Processing message to start a new season: {sqsMessage.Body}.");
                if (!TryDeserializeBody<SNSEvent.SNSMessage>(sqsMessage.Body, out var snsMessage, out var snsDeserializationError))
                {
                    context.Logger.LogLine(snsDeserializationError);
                    continue;
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

            var createFinalsMessage = JsonConvert.SerializeObject(newSeason);
            await _sqsClient.SendMessageAsync(CreateFinalQueueUrl, createFinalsMessage);

            context.Logger.LogLine($"Message {createFinalsMessage} was sent to create finals.");
        }
    }
}
