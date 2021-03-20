using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Function
{
    public abstract class DynamoFunction
    {
        protected AmazonDynamoDBClient DbClient { get; }
        protected DynamoDBContext DbContext { get; }

        protected DynamoFunction()
        {
            DbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig());
            DbContext = new DynamoDBContext(DbClient, new DynamoDBContextConfig());
        }
    }
}
