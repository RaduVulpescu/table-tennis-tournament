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

        public int Number { get; set; }
        public DateTime? EndDate { get; set; }

        public static Season Create(int number)
        {
            var instance = new Season
            {
                PK = $"SEASON#{Guid.NewGuid()}",
                Number = number
            };

            return instance;
        }
    }
}
