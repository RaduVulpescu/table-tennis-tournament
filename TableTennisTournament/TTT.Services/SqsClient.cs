using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace TTT.ExternalServices
{
    public class SqsClient : ISqsClient
    {
        private readonly AmazonSQSClient _sqsClient;

        public SqsClient()
        {
            _sqsClient = new AmazonSQSClient(new AmazonSQSConfig());
        }

        public Task<SendMessageResponse> SendMessageAsync(string queueUrl, string message)
        {
            return _sqsClient.SendMessageAsync(queueUrl, message);
        }
    }
}
