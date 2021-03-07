using System;
using Amazon.DynamoDBv2.DataModel;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("players")]
    public class Player : IEntity
    {
        [DynamoDBHashKey]
        public string PK { get; set; }

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
        public Levels CurrentLevel { get; set; }
        [DynamoDBProperty]
        public Levels? BestLevel { get; set; }

        [DynamoDBProperty]
        public int EliteCups { get; set; }
        [DynamoDBProperty]
        public int OpenCups { get; set; }
        [DynamoDBProperty]
        public int AdvancedCups { get; set; }
        [DynamoDBProperty]
        public int IntermediateCups { get; set; }
        [DynamoDBProperty]
        public int BeginnerCups { get; set; }
        [DynamoDBProperty]
        public int HobbyCups { get; set; }

        [DynamoDBProperty]
        public int EliteSeasons { get; set; }
        [DynamoDBProperty]
        public int OpenSeasons { get; set; }
        [DynamoDBProperty]
        public int AdvancedSeasons { get; set; }
        [DynamoDBProperty]
        public int IntermediateSeasons { get; set; }
        [DynamoDBProperty]
        public int BeginnerSeasons { get; set; }
        [DynamoDBProperty]
        public int HobbySeasons { get; set; }

        public static Player Create(string name, int? birthYear = null, string city = null,
            Levels currentLevel = Levels.Beginner, int? height = null, int? weight = null)
        {
            var instance = new Player
            {
                PK = $"PLAYER#{Guid.NewGuid()}"
            };

            instance.Update(name, birthYear, city, currentLevel, height, weight);

            return instance;
        }

        public void Update(string name, int? birthYear, string city, Levels currentLevel, int? height, int? weight)
        {
            Name = name;
            BirthYear = birthYear;
            City = city;
            CurrentLevel = currentLevel;
            Height = height;
            Weight = weight;
        }
    }
}
