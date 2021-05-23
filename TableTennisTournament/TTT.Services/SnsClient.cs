using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace TTT.Services
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
    }
}
