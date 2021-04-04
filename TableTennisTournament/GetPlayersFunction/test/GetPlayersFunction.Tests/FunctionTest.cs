using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using TTT.DomainModel.Entities;
using TTT.Players.Repository;
using Xunit;

namespace GetPlayersFunction.Tests
{
    public class FunctionTest
    {
        private readonly IPlayerRepository _playerRepository;

        public FunctionTest()
        {
            var playerRepositoryMock = new Mock<IPlayerRepository>();
            playerRepositoryMock
                .Setup(x => x.ListAsync())
                .ReturnsAsync(_players);

            _playerRepository = playerRepositoryMock.Object;
        }

        [Fact]
        public async Task GetPlayersFunction_WhenCalled_ReturnsOK()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest();

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, actualResponse.StatusCode);
            Assert.Contains("Radu", actualResponse.Body);
            Assert.Contains("Daniel", actualResponse.Body);
        }

        private Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function(_playerRepository);
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }

        private readonly List<Player> _players = new List<Player> { new Player { Name = "Radu" }, new Player { Name = "Daniel" } };
    }
}
