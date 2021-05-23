using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SimpleNotificationService.Model;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.Seasons.Repository;
using TTT.Services;
using Xunit;

namespace PatchEndSeasonFunction.Tests
{
    public class FunctionTest
    {
        private const string ExistingSeason = "4b2e2992-0dec-40ee-ac89-e2d5d35d363b";
        private readonly ISeasonRepository _seasonRepository;
        private readonly ISnsClient _snsClient;

        public FunctionTest()
        {
            var seasonRepositoryMock = new Mock<ISeasonRepository>();
            var snsClientMock = new Mock<ISnsClient>();

            seasonRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<Season>()))
                .Returns(Task.CompletedTask);

            seasonRepositoryMock
                .Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Season)null);

            seasonRepositoryMock
                .Setup(x => x.LoadAsync($"SEASON#{ExistingSeason}", $"SEASON_DATA#{ExistingSeason}"))
                .ReturnsAsync(new Season());

            _seasonRepository = seasonRepositoryMock.Object;

            snsClientMock
                .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PublishResponse());

            _snsClient = snsClientMock.Object;
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithMalformedInput_ReturnsUnsupportedMediaType()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = "not a valid json"
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, actualResponse.StatusCode);
            Assert.Contains("Deserialization error", actualResponse.Body);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithFutureEndDate_ReturnsBadRequest()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddMonths(1)
            };

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch)
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.Contains("'End Date' must be less than or equal to", actualResponse.Body);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithNonExistingSeasonId_ReturnsNotFound()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddDays(-1)
            };

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch),
                PathParameters = new Dictionary<string, string> { { "seasonId", Guid.NewGuid().ToString() } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithValidInput_ReturnsNoContent()
        {
            // Arrange
            var (function, context) = InitializeFunctionAndTestContext();
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddDays(-1)
            };

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch),
                PathParameters = new Dictionary<string, string> { { "seasonId", ExistingSeason } }
            };

            // Act
            var actualResponse = await function.FunctionHandler(request, context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }

        private Tuple<Function, TestLambdaContext> InitializeFunctionAndTestContext()
        {
            var function = new Function(_seasonRepository, _snsClient);
            var context = new TestLambdaContext();

            return new Tuple<Function, TestLambdaContext>(function, context);
        }
    }
}
