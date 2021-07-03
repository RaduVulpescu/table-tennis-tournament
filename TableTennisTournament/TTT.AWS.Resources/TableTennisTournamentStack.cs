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
    // ReSharper disable UnusedVariable
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
            var createFinalsQueue = new Queue(this, "CreateFinalsQueue");
            var notifySeasonEndedQueue = new Queue(this, "NotifySeasonEndedQueue");

            var endSeasonTopic = new Topic(this, "EndSeasonTopic", new TopicProps
            {
                DisplayName = "End Season subscription topic",
                TopicName = "endSeasonTopic"
            });

            endSeasonTopic.AddSubscription(new SqsSubscription(startSeasonQueue));
            endSeasonTopic.AddSubscription(new SqsSubscription(updatePlayersStatsQueue));
            endSeasonTopic.AddSubscription(new SqsSubscription(notifySeasonEndedQueue));

            var registerDeviceTokenFunction = CreateFunction("register-device-token-function", "RegisterDeviceTokenFunction");

            var sendNotificationFunction = CreateFunction("send-notification-function", "SendNotificationFunction");
            notifySeasonEndedQueue.GrantConsumeMessages(sendNotificationFunction);
            sendNotificationFunction.AddEventSource(new SqsEventSource(notifySeasonEndedQueue));

            var getPlayersFunction = CreateFunction("get-players-function", "GetPlayersFunction");
            table.GrantDescribeReadData(getPlayersFunction);

            var getPlayerFunction = CreateFunction("get-player-function", "GetPlayerFunction");
            table.GrantDescribeReadData(getPlayerFunction);

            var postPlayerFunction = CreateFunction("post-player-function", "PostPlayerFunction");
            table.GrantDescribeWriteData(postPlayerFunction);

            var putPlayerFunction = CreateFunction("put-player-function", "PutPlayerFunction");
            table.GrantDescribeReadWriteData(putPlayerFunction);

            var deletePlayerFunction = CreateFunction("delete-player-function", "DeletePlayerFunction");
            table.GrantDescribeReadWriteData(deletePlayerFunction);

            var endSeasonFunction = CreateFunction("end-season-function", "PatchEndSeasonFunction");
            table.GrantDescribeReadWriteData(endSeasonFunction);
            endSeasonTopic.GrantPublish(endSeasonFunction);

            var startSeasonFunction = CreateFunction("start-season-function", "SQSEventStartSeasonFunction");
            table.GrantDescribeWriteData(startSeasonFunction);
            startSeasonQueue.GrantConsumeMessages(startSeasonFunction);
            startSeasonFunction.AddEventSource(new SqsEventSource(startSeasonQueue));
            createFinalsQueue.GrantSendMessages(startSeasonFunction);

            var updatePlayerStatsFunction = CreateFunction("update-players-stats-function", "SQSEventUpdatePlayersStatsFunction");
            table.GrantDescribeReadWriteData(updatePlayerStatsFunction);
            updatePlayersStatsQueue.GrantConsumeMessages(updatePlayerStatsFunction);
            updatePlayerStatsFunction.AddEventSource(new SqsEventSource(updatePlayersStatsQueue));

            var createFinalsFunction = CreateFunction("create-finals-function", "SQSEventCreateFinalsFunction");
            table.GrantDescribeWriteData(createFinalsFunction);
            createFinalsQueue.GrantConsumeMessages(createFinalsFunction);
            createFinalsFunction.AddEventSource(new SqsEventSource(createFinalsQueue));

            var getSeasonsFunction = CreateFunction("get-seasons-function", "GetSeasonsFunction");
            table.GrantDescribeReadData(getSeasonsFunction);

            var getSeasonPlayersFunction = CreateFunction("get-seasons-players-function", "GetSeasonPlayersFunction");
            table.GrantDescribeReadData(getSeasonPlayersFunction);

            var getUpcomingFixturesFunction = CreateFunction("get-upcoming-fixtures-function", "GetUpcomingFixturesFunction");
            table.GrantDescribeReadData(getUpcomingFixturesFunction);

            var postFixtureFunction = CreateFunction("post-fixture-function", "AddFixtureFunction");
            table.GrantDescribeReadWriteData(postFixtureFunction);

            var putFixtureFunction = CreateFunction("put-fixture-function", "PutFixtureFunction");
            table.GrantDescribeReadWriteData(putFixtureFunction);

            var startFixtureFunction = CreateFunction("start-fixture-function", "StartFixtureFunction");
            table.GrantDescribeReadWriteData(startFixtureFunction);

            var patchGroupMatchFunction = CreateFunction("patch-group-match-function", "PatchGroupMatchFunction");
            table.GrantDescribeReadWriteData(patchGroupMatchFunction);

            var endGroupStageFunction = CreateFunction("end-group-stage-function", "EndGroupStageFunction");
            table.GrantDescribeReadWriteData(endGroupStageFunction);

            var patchDeciderMatchFunction = CreateFunction("patch-decider-match-function", "PatchDeciderMatchFunction");
            table.GrantDescribeReadWriteData(patchDeciderMatchFunction);

            var endFixtureFunction = CreateFunction("end-fixture-function", "EndFixtureFunction");
            table.GrantDescribeReadWriteData(endFixtureFunction);

            var httpApi = new HttpApi(this, "ttt-http-api", new HttpApiProps
            {
                ApiName = "ttt-api"
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/aws/platformApplication/endpoints",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = registerDeviceTokenFunction
                })
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
                Path = "/seasons/{seasonId}/endSeason",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = endSeasonFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons",
                Methods = new[] { HttpMethod.GET },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = getSeasonsFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/players",
                Methods = new[] { HttpMethod.GET },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = getSeasonPlayersFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures",
                Methods = new[] { HttpMethod.GET },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = getUpcomingFixturesFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = postFixtureFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}",
                Methods = new[] { HttpMethod.PUT },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = putFixtureFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}/start",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = startFixtureFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}/groupMatches/{matchId}",
                Methods = new[] { HttpMethod.PATCH },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = patchGroupMatchFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}/endGroupStage",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = endGroupStageFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}/deciderMatches/{matchId}",
                Methods = new[] { HttpMethod.PATCH },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = patchDeciderMatchFunction
                })
            });

            httpApi.AddRoutes(new AddRoutesOptions
            {
                Path = "/seasons/{seasonId}/fixtures/{fixtureId}/endFixture",
                Methods = new[] { HttpMethod.POST },
                Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps
                {
                    Handler = endFixtureFunction
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

    internal static class Extensions
    {
        public static void GrantDescribeReadData(this Table table, IGrantable grantee)
        {
            table.GrantDescribeTable(grantee);
            table.GrantReadData(grantee);
        }

        public static void GrantDescribeWriteData(this Table table, IGrantable grantee)
        {
            table.GrantDescribeTable(grantee);
            table.GrantWriteData(grantee);
        }
        public static void GrantDescribeReadWriteData(this Table table, IGrantable grantee)
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
