using System;
using Amazon.DynamoDBv2.DataModel;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("table-tennis-tournament")]
    public class PlayerMatch : IDynamoItem
    {
        [DynamoDBHashKey]
        public string PK { get; set; }
        [DynamoDBRangeKey]
        public string SK { get; set; }

        public Guid MatchId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid OpponentId { get; set; }
        public int SetsWonByThePlayer { get; set; }
        public int SetsWonByTheOpponent { get; set; }
        public bool IsWin { get; set; }
    }
}
