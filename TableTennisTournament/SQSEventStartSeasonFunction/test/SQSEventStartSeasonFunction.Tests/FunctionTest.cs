using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using TTT.DomainModel.Entities;
using TTT.Seasons.Repository;
using Xunit;

namespace SQSEventStartSeasonFunction.Tests
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
        public async Task SQSEventStartSeasonFunction_WithValidSeason_LogsFinishedProcessing()
        {
            // Arrange
            var (function, context, logger) = InitializeFunctionAndTestContext();
            var seasonToAdd = new Season
            {
                PK = "PK",
                SK = "SK",
                SeasonId = Guid.NewGuid(),
                Number = 1,
                StartDate = DateTime.Now.AddMonths(-2),
                EndDate = DateTime.Now
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(new SNSEvent.SNSMessage
                        {
                            Message = JsonConvert.SerializeObject(seasonToAdd)
                        })
                    }
                }
            };

            // Act
            await function.FunctionHandler(sqsEvent, context);

            // Assert
            Assert.Contains("Finished processing message to create a new season.", logger.Buffer.ToString());
        }

        [Fact]
        public async Task SQSEventStartSeasonFunction_WithInvalidInput_LogsDeserializationError()
        {
            // Arrange
            var (function, context, logger) = InitializeFunctionAndTestContext();

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(new SNSEvent.SNSMessage
                        {
                            Message = "not a season"
                        })
                    }
                }
            };

            // Act
            await function.FunctionHandler(sqsEvent, context);

            // Assert
            Assert.Contains("Deserialization error", logger.Buffer.ToString());
        }

        [Fact]
        public async Task SQSEventStartSeasonFunction_WithNoEndDate_LogsE()
        {
            // Arrange
            var (function, context, logger) = InitializeFunctionAndTestContext();
            var seasonToAdd = new Season();

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(new SNSEvent.SNSMessage
                        {
                            Message = JsonConvert.SerializeObject(seasonToAdd)
                        })
                    }
                }
            };

            // Act
            await function.FunctionHandler(sqsEvent, context);

            // Assert
            Assert.Contains($"Season with id {seasonToAdd.SeasonId} does not have an endDate set.", logger.Buffer.ToString());
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
