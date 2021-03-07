using System;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGatewayv2;
using Amazon.CDK.AWS.APIGatewayv2.Integrations;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.DynamoDB;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using Construct = Constructs.Construct;

namespace TTT.AWS.Resources
{
    public class TableTennisTournamentStack : Stack
    {
        private const string TargetFrameWork = "netcoreapp3.1";

        internal TableTennisTournamentStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var playersTable = new Table(this, "players", new TableProps
            {
                TableName = "players",
                PartitionKey = new Attribute { Name = "PK", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST
            });

            var postPlayerFunction = CreatePostPlayerFunction();

            playersTable.Grant(postPlayerFunction, "dynamodb:DescribeTable");
            playersTable.GrantWriteData(postPlayerFunction);

            var getPlayerFunction = CreateGetPlayerFunction();

            playersTable.Grant(getPlayerFunction, "dynamodb:DescribeTable");
            playersTable.GrantReadData(getPlayerFunction);

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
        }

        private Function CreateGetPlayerFunction()
        {
            const string functionName = "get-player-function";
            const string functionAssembly = "GetPlayerFunction";

            return new Function(this, functionName, new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_CORE_3_1,
                Handler = $"{functionAssembly}::{functionAssembly}.Function::FunctionHandler",
                Code = Code.FromAsset($"./TableTennisTournament/{functionAssembly}/src/{functionAssembly}/bin/Release/{TargetFrameWork}/publish")
            });
        }

        private Function CreatePostPlayerFunction()
        {
            const string functionName = "post-player-function";
            const string functionAssembly = "PostPlayerFunction";

            return new Function(this, functionName, new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_CORE_3_1,
                Handler = $"{functionAssembly}::{functionAssembly}.Function::FunctionHandler",
                Code = Code.FromAsset($"./TableTennisTournament/{functionAssembly}/src/{functionAssembly}/bin/Release/{TargetFrameWork}/publish")
            });
        }
    }
}
