using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.Seasons.Repository;
using Xunit;

namespace SQSEventCreateFinalsFunction.Tests
{
    public class FunctionTest
    {
        private readonly ISeasonRepository _seasonRepository;

        public FunctionTest()
        {
            var seasonRepository = new Mock<ISeasonRepository>();

            seasonRepository
                .Setup(x => x.SaveAsync(It.IsAny<Season>()))
                .Returns(Task.CompletedTask);

            _seasonRepository = seasonRepository.Object;
        }

        [Fact]
        public async Task SQSEventCreateFinalsFunction_WithValidInput_CreatesAllFinals()
        {
            // Arrange
            var (function, context, logger) = InitializeFunctionAndTestContext();
            var newlyCreatedSeason = new Season
            {
                PK = "PK",
                SK = "SK",
                SeasonId = Guid.NewGuid(),
                Number = 2,
                StartDate = DateTime.Now
            };
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(newlyCreatedSeason)
                    }
                }
            };


            // Act
            await function.FunctionHandler(sqsEvent, context);

            // Assert
            Assert.Contains("Finished processing message to create all finals.", logger.Buffer.ToString());
        }

        private Tuple<Function, TestLambdaContext, TestLambdaLogger> InitializeFunctionAndTestContext()
        {
            var function = new Function(_seasonRepository);
            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            return new Tuple<Function, TestLambdaContext, TestLambdaLogger>(function, context, logger);
        }
    }
}
