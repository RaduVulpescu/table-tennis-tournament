using Amazon.CDK;
using Amazon.CDK.AWS.APIGatewayv2;
using Amazon.CDK.AWS.APIGatewayv2.Integrations;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using Construct = Constructs.Construct;

namespace TTT.AWS.Resources
{
    public class TableTennisTournamentStack : Stack
    {
        private const string TargetFrameWork = "netcoreapp3.1";

        internal TableTennisTournamentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var table = new Table(this, "table-tennis-tournament", new TableProps
            {
                TableName = "table-tennis-tournament",
                PartitionKey = new Attribute { Name = "PK", Type = AttributeType.STRING },
                SortKey = new Attribute { Name = "SK", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var startSeasonQueue = new Queue(this, "StartSeasonQueue");
            var updatePlayersStatsQueue = new Queue(this, "UpdatePlayerStatsQueue");

            var endSeasonTopic = new Topic(this, "EndSeasonTopic", new TopicProps
            {
                DisplayName = "End Season subscription topic",
                TopicName = "endSeasonTopic"
            });

            endSeasonTopic.AddSubscription(new SqsSubscription(startSeasonQueue));
            endSeasonTopic.AddSubscription(new SqsSubscription(updatePlayersStatsQueue));

            var getPlayersFunction = CreateFunction("get-players-function", "GetPlayersFunction");
            table.GrantCustomReadData(getPlayersFunction);

            var getPlayerFunction = CreateFunction("get-player-function", "GetPlayerFunction");
            table.GrantCustomReadData(getPlayerFunction);

            var postPlayerFunction = CreateFunction("post-player-function", "PostPlayerFunction");
            table.GrantCustomWriteData(postPlayerFunction);

            var putPlayerFunction = CreateFunction("put-player-function", "PutPlayerFunction");
            table.GrantCustomReadWriteData(putPlayerFunction);

            var deletePlayerFunction = CreateFunction("delete-player-function", "DeletePlayerFunction");
            table.GrantCustomReadWriteData(deletePlayerFunction);

            var endSeasonFunction = CreateFunction("end-season-function", "PatchEndSeasonFunction");
            table.GrantCustomReadWriteData(endSeasonFunction);
            endSeasonTopic.GrantPublish(endSeasonFunction);

            var startSeasonFunction = CreateFunction("start-season-function", "SQSEventStartSeasonFunction");
            startSeasonQueue.GrantConsumeMessages(startSeasonFunction);
            startSeasonFunction.AddEventSource(new SqsEventSource(startSeasonQueue));

            var httpApi = new HttpApi(this, "ttt-http-api", new HttpApiProps
            {
                ApiName = "ttt-api"
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/players",
                Methods = new[] { HttpMethod.GET },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = getPlayersFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/players/{playerId}",
                Methods = new[] { HttpMethod.GET },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = getPlayerFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/players",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = postPlayerFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/players/{playerId}",
                Methods = new[] { HttpMethod.PUT },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = putPlayerFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/players/{playerId}",
                Methods = new[] { HttpMethod.DELETE },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = deletePlayerFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}",
                Methods = new[] { HttpMethod.PATCH },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = endSeasonFunction
                })
            });
        }

        private Function CreateFunction(string functionName, string functionAssembly)
        {
            return new Function(this, functionName, new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_CORE_3_1,
                Handler = $"{functionAssembly}::{functionAssembly}.Function::FunctionHandler",
                Code = Code.FromAsset($"./TableTennisTournament/{functionAssembly}/src/{functionAssembly}/bin/Release/{TargetFrameWork}"),
                Timeout = Duration.Seconds(30)
            });
        }
    }

    public static class Extensions
    {
        public static void GrantCustomReadData(this Table table, IGrantable grantee)
        {
            table.GrantDescribeTable(grantee);
            table.GrantReadData(grantee);
        }

        public static void GrantCustomWriteData(this Table table, IGrantable grantee)
        {
            table.GrantDescribeTable(grantee);
            table.GrantWriteData(grantee);
        }
        public static void GrantCustomReadWriteData(this Table table, IGrantable grantee)
        {
            table.GrantDescribeTable(grantee);
            table.GrantReadWriteData(grantee);
        }

        private static void GrantDescribeTable(this ITable table, IGrantable grantee)
        {
            table.Grant(grantee, "dynamodb:DescribeTable");
        }
    }
}
