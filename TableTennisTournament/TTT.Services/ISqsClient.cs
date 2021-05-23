using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace TTT.Services
{
    public interface ISqsClient
    {
        Task<SendMessageResponse> SendMessageAsync(string queueUrl, string message);
    }
}
