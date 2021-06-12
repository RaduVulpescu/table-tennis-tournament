using System;
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
        public async Task Seed_Player()
        {
            const int numberOfPlayersToCreate = 30;

            for (var i = 0; i < numberOfPlayersToCreate; i++)
            {
                await _seedFunction.CreatePlayerAsync(CreatePlayer());
            }
        }

        //[Fact]
        public async Task Seed_SeasonPlayer()
        {
            const int numberOfPlayersToCreate = 30;

            for (var i = 0; i < numberOfPlayersToCreate; i++)
            {
                await _seedFunction.CreateSeasonPlayerAsync(CreateSeasonPlayer(i));
            }
        }

        private Player CreatePlayer()
        {
            var playerId = Guid.NewGuid();

            var random = new Random();
            var randomIndex1 = random.Next(0, _namePool.Length);
            var randomIndex2 = random.Next(0, _namePool.Length);

            var randomLevel = random.Next(2, 6);

            return new Player
            {
                PK = Player.CreatePK(playerId.ToString()),
                SK = Player.CreateSK(playerId.ToString()),
                PlayerId = playerId,
                City = "Iasi",
                Name = $"{_namePool[randomIndex1]} {_namePool[randomIndex2]}",
                CurrentLevel = (Level)randomLevel,
                BestLevel = (Level)randomLevel,
                Quality = GetRandomDouble(68, 80),
                BirthYear = random.Next(1970, 2000),
                BestScore = GetRandomDouble(280, 320),
                BestTop4 = GetRandomDouble(280, 320),
                BestRanking = random.Next(1, 50)
            };
        }

        private SeasonPlayer CreateSeasonPlayer(int index)
        {
            var seasonId = Guid.Parse("e1c999e6-baff-4d85-9205-4d4f806812ad");
            var playerId = Guid.NewGuid();

            var random = new Random();
            var randomIndex1 = random.Next(0, _namePool.Length);
            var randomIndex2 = random.Next(0, _namePool.Length);

            var randomLevel = random.Next(2, 6);

            return new SeasonPlayer
            {
                PK = SeasonPlayer.CreatePK(seasonId.ToString()),
                SK = SeasonPlayer.CreateSK(playerId.ToString()),
                SeasonId = seasonId,
                PlayerId = playerId,
                Rank = index + 1,
                Name = $"{_namePool[randomIndex1]} {_namePool[randomIndex2]}",
                Level = (Level)randomLevel,
                Quality = GetRandomDouble(68, 80),
                Score1 = GetRandomDouble(68, 80),
                Score2 = GetRandomDouble(68, 80),
                Score3 = GetRandomDouble(68, 80),
                Score4 = GetRandomDouble(68, 80),
                Shape = GetRandomDouble(-0.05, 0.05)
            };
        }

        private readonly string[] _namePool =
        {
            "Shaithis",
            "Puscas",
            "Serban",
            "Torje",
            "Shaithis",
            "Andreescu",
            "Petru",
            "Negutesco",
            "Pereteanu",
            "Tomoiaga",
            "Serban",
            "Baicu",
            "Costica",
            "Ragar",
            "Velkan",
            "Grigorescu",
            "Bodgan",
            "Andreescu",
            "Costin",
            "Gogean",
            "Valentina",
            "Silivasi",
            "Valentina",
            "Raceanu",
            "Estera",
            "Belododia",
            "Teodora",
            "Silivasi",
            "Sorinah",
            "Georghiou",
            "Anica",
            "Macedonski",
            "Romanita",
            "Ardelean",
            "Maria",
            "Romanescu",
            "Constansa",
            "Petri",
            "Jenica",
            "Bogza"
        };

        private static double GetRandomDouble(double minimum, double maximum)
        {
            var random = new Random();
            var rDouble = random.NextDouble() * (maximum - minimum) + minimum;
            return Math.Round(rDouble, 2);
        }
    }
}
