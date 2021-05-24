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
                PK = CreatePK(newGuid.ToString()),
                SK = CreateSK(newGuid.ToString()),
                SeasonId = newGuid,
                Number = number,
                StartDate = startDate
            };

            return instance;
        }

        public static string CreatePK(string seasonId)
        {
            return $"{Constants.SeasonPrefix}#{seasonId}";
        }

        public static string CreateSK(string seasonId)
        {
            return $"{Constants.SeasonDataPrefix}#{seasonId}";
        }
    }
}
