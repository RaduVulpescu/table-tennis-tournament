using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using Xunit;

namespace DataSeed.Tests
{
    public class FunctionTest
    {
        private readonly Function _seedFunction;
        private readonly TestLambdaContext _testContext;

        public FunctionTest()
        {
            _seedFunction = new Function();
            _testContext = new TestLambdaContext();
        }

        //[Fact]
        public async Task Seed_Season()
        {
            // Arrange
            var season = Season.Create(1, DateTime.Now);
            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Body = JsonConvert.SerializeObject(season)
            };

            // Act
            var actualResponse = await _seedFunction.CreateSeasonAsync(request, _testContext);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, actualResponse.StatusCode);
        }

        //[Fact]
        public async Task Seed_SeasonPlayer()
        {
            const int numberOfPlayersToCreate = 5;

            for (var i = 0; i < numberOfPlayersToCreate; i++)
            {
                await _seedFunction.CreateSeasonPlayerAsync(CreatePlayer());
            }
        }

        private static SeasonPlayer CreatePlayer()
        {
            var seasonId = Guid.Parse("e1c999e6-baff-4d85-9205-4d4f806812ad");
            var playerId = Guid.NewGuid();

            return new SeasonPlayer
            {
                PK = SeasonPlayer.CreatePK(seasonId.ToString()),
                SK = SeasonPlayer.CreateSK(playerId.ToString()),
                SeasonId = seasonId,
                PlayerId = playerId,
                Rank = 3,
                Name = "Dan Epure",
                Level = Level.Intermediate,
                Quality = 67.67d,
                Score1 = 74.11d,
                Score2 = 73.12d,
                Score3 = 72.13d,
                Score4 = 71.14d,
                Shape = -0.0505d,
            };
        }
    }
}
