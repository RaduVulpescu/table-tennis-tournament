using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService.Model;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace RegisterDeviceTokenFunction
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
            if (!TryDeserializeBody<DeviceInformation>(request.Body, out var deviceInformation, out var error))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = error,
                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType
                };
            }

            var createPlatformEndpointRequest = new CreatePlatformEndpointRequest
            {
                Token = deviceInformation.Token,
                PlatformApplicationArn = "arn:aws:sns:eu-west-1:623072768925:app/GCM/ttt-push-notification-app",
                CustomUserData = $"{DateTime.Now}"
            };

            await _snsClient.CreatePlatformEndpointAsync(createPlatformEndpointRequest);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }

    public class DeviceInformation
    {
        public string Token { get; set; }
    }
}
