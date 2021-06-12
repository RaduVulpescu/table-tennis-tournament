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

        public Guid PlayerId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public int? BirthYear { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }

        public double? Quality { get; set; }
        public double? BestScore { get; set; }
        public int? BestRanking { get; set; }
        public double? BestTop4 { get; set; }
        public Level CurrentLevel { get; set; }
        public Level BestLevel { get; set; }

        public int OpenCups { get; set; }
        public int AdvancedCups { get; set; }
        public int IntermediateCups { get; set; }
        public int BeginnerCups { get; set; }

        public int OpenSeasons { get; set; }
        public int AdvancedSeasons { get; set; }
        public int IntermediateSeasons { get; set; }
        public int BeginnerSeasons { get; set; }

        public static Player Create(string name, string city = null, int? birthYear = null, int? height = null, int? weight = null,
            Level currentLevel = Level.Beginner)
        {
            var newGuid = Guid.NewGuid();

            var instance = new Player
            {
                PK = CreatePK(newGuid.ToString()),
                SK = CreateSK(newGuid.ToString()),
                PlayerId = newGuid
            };

            instance.Update(name, city, birthYear, height, weight, currentLevel);

            return instance;
        }

        public void Update(string name, string city, int? birthYear, int? height, int? weight, Level currentLevel, Level bestLevel = Level.Undefined)
        {
            Name = name;
            City = city;
            BirthYear = birthYear;
            Height = height;
            Weight = weight;
            CurrentLevel = currentLevel;
            BestLevel = bestLevel;
        }

        public static string CreatePK(string playerId)
        {
            return $"{Constants.PlayerPrefix}#{playerId}";
        }

        public static string CreateSK(string playerId)
        {
            return $"{Constants.PlayerDataPrefix}#{playerId}";
        }
    }
}
