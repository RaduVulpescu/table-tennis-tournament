using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService.Model;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.ExternalServices;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberHidesStaticFromOuterClass
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace SendNotificationFunction
{
    public class Function : BaseFunction
    {
        private readonly ISnsClient _snsClient;

        public Function()
        {
            _snsClient = ServiceProvider.GetService<ISnsClient>();
        }

        public Function(ISnsClient snsClient)
        {
            _snsClient = snsClient;
        }

        public async Task FunctionHandler(SQSEvent ev, ILambdaContext context)
        {
            foreach (var sqsMessage in ev.Records)
            {
                context.Logger.LogLine($"Processing message to notify devices that season ended: {sqsMessage.Body}.");
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

            var gcm = new GCM
            {
                notification = new Notification
                {
                    title = $"Season {finishedSeason.Number} is finished!",
                    body = "Check out who is the winner!"
                }
            };

            var pushNotificationMessage = new PushNotificationMessage
            {
                GCM = JsonConvert.SerializeObject(gcm)
            };

            var listEndpointsRequest = new ListEndpointsByPlatformApplicationRequest
            {
                PlatformApplicationArn = "arn:aws:sns:eu-west-1:623072768925:app/GCM/ttt-push-notification-app",
                NextToken = null
            };

            var endpointsResponse = await _snsClient.GetEndpointsAsync(listEndpointsRequest);
            foreach (var endpoint in endpointsResponse.Endpoints)
            {
                await _snsClient.PublishAsync(new PublishRequest
                {
                    TargetArn = endpoint.EndpointArn,
                    Message = JsonConvert.SerializeObject(pushNotificationMessage),
                    MessageStructure = "json"
                });
            }
        }

        internal class PushNotificationMessage
        {
            public string GCM { get; set; }
        }

        internal class GCM
        {
            public Notification notification { get; set; }
        }

        internal class Notification
        {
            public string title { get; set; }
            public string body { get; set; }
        }
    }
}
