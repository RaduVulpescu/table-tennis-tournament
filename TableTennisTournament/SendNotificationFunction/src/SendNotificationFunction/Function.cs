using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService.Model;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TTT.ExternalServices;

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

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            if (!TryDeserializeBody<Notification>(request.Body, out var notification, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            var gcm = new GCM
            {
                notification = new Notification
                {
                    title = notification.title,
                    body = notification.body
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

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
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
