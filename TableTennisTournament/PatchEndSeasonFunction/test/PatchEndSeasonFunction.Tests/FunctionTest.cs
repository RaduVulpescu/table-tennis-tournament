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
using TTT.ExternalServices;
using TTT.Seasons.Repository;
using Xunit;

namespace PatchEndSeasonFunction.Tests
{
    public class FunctionTest
    {
        private readonly Mock<ISeasonRepository> _seasonRepositoryMock;
        private readonly Mock<ISnsClient> _snsClientMock;
        private readonly Mock<ISqsClient> _sqsClientMock;
        private readonly Function _sutFunction;
        private readonly TestLambdaContext _testContext;

        private const string ExistingSeasonId = "4b2e2992-0dec-40ee-ac89-e2d5d35d363b";
        private const string SeasonIdQueryParamKey = "seasonId";

        public FunctionTest()
        {
            _seasonRepositoryMock = new Mock<ISeasonRepository>();
            _snsClientMock = new Mock<ISnsClient>();
            _sqsClientMock = new Mock<ISqsClient>();

            _seasonRepositoryMock
                .Setup(x => x.SaveAsync(It.IsAny<Season>()))
                .Returns(Task.CompletedTask);

            _seasonRepositoryMock
                .Setup(x => x.LoadSeasonAsync(It.IsAny<string>()))
                .ReturnsAsync((Season)null);

            _seasonRepositoryMock
                .Setup(x => x.LoadSeasonAsync(ExistingSeasonId))
                .ReturnsAsync(new Season());

            _snsClientMock
                .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PublishResponse());

            _sutFunction = new Function(_seasonRepositoryMock.Object, _snsClientMock.Object);
            _testContext = new TestLambdaContext();
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithMalformedInput_ReturnsUnsupportedMediaType()
        {
            // Arrange
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = "not a valid json"
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, actualResponse.StatusCode);
            Assert.Contains("Deserialization error", actualResponse.Body);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithFutureEndDate_ReturnsBadRequest()
        {
            // Arrange
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddMonths(1)
            };

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch)
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.VerifyNoOtherCalls();
            Assert.Equal((int)HttpStatusCode.BadRequest, actualResponse.StatusCode);
            Assert.Contains("'End Date' must be less than or equal to", actualResponse.Body);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithNonExistingSeasonId_ReturnsNotFound()
        {
            // Arrange
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddDays(-1)
            };
            var nonExistingSeasonId = Guid.NewGuid().ToString();
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch),
                PathParameters = new Dictionary<string, string> { { SeasonIdQueryParamKey, nonExistingSeasonId } }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _seasonRepositoryMock.Verify(x => x.LoadSeasonAsync(nonExistingSeasonId), Times.Once);
            Assert.Equal((int)HttpStatusCode.NotFound, actualResponse.StatusCode);
        }

        [Fact]
        public async Task PatchEndSeasonFunction_WithValidInput_ReturnsNoContent()
        {
            // Arrange
            var seasonPatch = new SeasonsPatchDTO
            {
                EndDate = DateTime.Now.AddDays(-1)
            };

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(seasonPatch),
                PathParameters = new Dictionary<string, string> { { SeasonIdQueryParamKey, ExistingSeasonId } }
            };

            // Act
            var actualResponse = await _sutFunction.FunctionHandler(request, _testContext);

            // Assert
            _snsClientMock.Verify(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None), Times.Once);
            Assert.Equal((int)HttpStatusCode.NoContent, actualResponse.StatusCode);
        }
    }
}
