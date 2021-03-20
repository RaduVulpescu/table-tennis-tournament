using Amazon.CDK;
using Amazon.CDK.AWS.APIGatewayv2;
using Amazon.CDK.AWS.APIGatewayv2.Integrations;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
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

            var getPlayersFunction = CreateFunction("get-players-function", "GetPlayersFunction");
            table.Grant(getPlayersFunction, "dynamodb:DescribeTable");
            table.GrantReadData(getPlayersFunction);

            var getPlayerFunction = CreateFunction("get-player-function", "GetPlayerFunction");
            table.Grant(getPlayerFunction, "dynamodb:DescribeTable");
            table.GrantReadData(getPlayerFunction);

            var postPlayerFunction = CreateFunction("post-player-function", "PostPlayerFunction");
            table.Grant(postPlayerFunction, "dynamodb:DescribeTable");
            table.GrantWriteData(postPlayerFunction);

            var putPlayerFunction = CreateFunction("put-player-function", "PutPlayerFunction");
            table.Grant(putPlayerFunction, "dynamodb:DescribeTable");
            table.GrantWriteData(putPlayerFunction);

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
        }

        private Function CreateFunction(string functionName, string functionAssembly)
        {
            return new Function(this, functionName, new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_CORE_3_1,
                Handler = $"{functionAssembly}::{functionAssembly}.Function::FunctionHandler",
                Code = Code.FromAsset($"./TableTennisTournament/{functionAssembly}/src/{functionAssembly}/bin/Release/{TargetFrameWork}/publish"),
                Timeout = Duration.Seconds(30)
            });
        }
    }
}
