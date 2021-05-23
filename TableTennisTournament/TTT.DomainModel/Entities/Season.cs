using System;
using Amazon.DynamoDBv2.DataModel;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("table-tennis-tournament")]
    public class Season : IDynamoItem
    {
        [DynamoDBHashKey]
        public string PK { get; set; }
        [DynamoDBRangeKey]
        public string SK { get; set; }

        public Guid SeasonId { get; set; }
        public int Number { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public static Season Create(int number, DateTime startDate)
        {
            var newGuid = Guid.NewGuid();

            var instance = new Season
            {
                PK = $"SEASON#{newGuid}",
                SK = $"SEASON_DATA#{newGuid}",
                SeasonId = newGuid,
                Number = number,
                StartDate = startDate
            };

            return instance;
        }
    }
}
