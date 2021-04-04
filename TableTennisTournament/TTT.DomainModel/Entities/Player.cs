using System;
using Amazon.DynamoDBv2.DataModel;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("table-tennis-tournament")]
    public class Player : IDynamoItem
    {
        [DynamoDBHashKey]
        public string PK { get; set; }
        [DynamoDBRangeKey]
        public string SK { get; set; }

        [DynamoDBProperty]
        public Guid PlayerId { get; set; }
        [DynamoDBProperty]
        public string Name { get; set; }
        [DynamoDBProperty]
        public string City { get; set; }
        [DynamoDBProperty]
        public int? BirthYear { get; set; }
        [DynamoDBProperty]
        public int? Height { get; set; }
        [DynamoDBProperty]
        public int? Weight { get; set; }

        [DynamoDBProperty]
        public double? Quality { get; set; }
        [DynamoDBProperty]
        public double? BestScore { get; set; }
        [DynamoDBProperty]
        public int? BestRanking { get; set; }
        [DynamoDBProperty]
        public double? BestTop4 { get; set; }
        [DynamoDBProperty]
        public Level CurrentLevel { get; set; }
        [DynamoDBProperty]
        public Level? BestLevel { get; set; }

        [DynamoDBProperty]
        public int OpenCups { get; set; }
        [DynamoDBProperty]
        public int AdvancedCups { get; set; }
        [DynamoDBProperty]
        public int IntermediateCups { get; set; }
        [DynamoDBProperty]
        public int BeginnerCups { get; set; }

        [DynamoDBProperty]
        public int OpenSeasons { get; set; }
        [DynamoDBProperty]
        public int AdvancedSeasons { get; set; }
        [DynamoDBProperty]
        public int IntermediateSeasons { get; set; }
        [DynamoDBProperty]
        public int BeginnerSeasons { get; set; }

        public static Player Create(string name, string city = null, int? birthYear = null, int? height = null, int? weight = null,
            Level currentLevel = Level.Beginner)
        {
            var newGuid = Guid.NewGuid();

            var instance = new Player
            {
                PK = $"PLAYER#{newGuid}",
                SK = $"PLAYERDATA#{newGuid}",
                PlayerId = newGuid
            };

            instance.Update(name, city, birthYear, height, weight, currentLevel);

            return instance;
        }

        public void Update(string name, string city, int? birthYear, int? height, int? weight, Level currentLevel)
        {
            Name = name;
            City = city;
            BirthYear = birthYear;
            Height = height;
            Weight = weight;
            CurrentLevel = currentLevel;
        }
    }
}
