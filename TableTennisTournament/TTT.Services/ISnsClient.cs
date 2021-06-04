using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;

namespace TTT.Services
{
    public interface ISnsClient
    {
        Task<PublishResponse> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default);
        Task<CreatePlatformEndpointResponse> CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request);
    }
}
