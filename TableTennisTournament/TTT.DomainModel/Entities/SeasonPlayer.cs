using System;
using Amazon.DynamoDBv2.DataModel;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("table-tennis-tournament")]
    public class SeasonPlayer : IDynamoItem
    {
        [DynamoDBHashKey]
        public string PK { get; set; }

        [DynamoDBRangeKey]
        public string SK { get; set; }

        public Guid SeasonId { get; set; }
        public Guid PlayerId { get; set; }

        public int Rank { get; set; }
        public string Name { get; set; }
        public Level Level { get; set; }
        public double Quality { get; set; }
        public double Top4 { get; set; }
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public double Score3 { get; set; }
        public double Score4 { get; set; }
        public double Shape { get; set; }

        public static SeasonPlayer Create(string seasonId, string playerId, string name, Level level, double quality, double score1)
        {
            return new SeasonPlayer
            {
                PK = CreatePK(seasonId),
                SK = CreateSK(playerId),
                SeasonId = Guid.Parse(seasonId),
                PlayerId = Guid.Parse(playerId),
                Name = name,
                Quality = quality,
                Level = level,
                Top4 = score1,
                Score1 = score1,
                Score2 = 0,
                Score3 = 0,
                Score4 = 0
            };
        }

        public static string CreatePK(string seasonId)
        {
            return $"{Constants.SeasonPrefix}#{seasonId}";
        }

        public static string CreateSK(string playerId)
        {
            return $"{Constants.PlayerPrefix}#{playerId}";
        }
    }
}
