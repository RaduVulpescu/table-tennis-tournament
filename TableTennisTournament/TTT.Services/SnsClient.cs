using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace TTT.ExternalServices
{
    public class SnsClient : ISnsClient
    {
        private readonly AmazonSimpleNotificationServiceClient _snsClient;

        public SnsClient()
        {
            _snsClient = new AmazonSimpleNotificationServiceClient();
        }

        public Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default)
        {
            return _snsClient.PublishAsync(request, cancellationToken);
        }

        public Task<CreatePlatformEndpointResponse> CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request)
        {
            return _snsClient.CreatePlatformEndpointAsync(request);
        }

        public Task<ListEndpointsByPlatformApplicationResponse> GetEndpointsAsync(ListEndpointsByPlatformApplicationRequest request)
        {
            return _snsClient.ListEndpointsByPlatformApplicationAsync(request);
        }
    }
}
