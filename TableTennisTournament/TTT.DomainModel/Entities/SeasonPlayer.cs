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
        public double Score1 { get; set; }
        public double Score2 { get; set; }
        public double Score3 { get; set; }
        public double Score4 { get; set; }
        public double Shape { get; set; }
    }
}
