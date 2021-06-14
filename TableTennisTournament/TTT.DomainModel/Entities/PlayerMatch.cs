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

        public Guid PlayerId { get; set; }
        public Guid MatchId { get; set; }
        public Guid OpponentId { get; set; }
        public string PlayerName { get; set; }
        public string OpponentName { get; set; }
        public int? SetsWonByThePlayer { get; set; }
        public int? SetsWonByTheOpponent { get; set; }
        public bool? IsWin { get; private set; }

        public static PlayerMatch Create(Guid matchId, Guid playerId, Guid opponentId, string playerName, string opponentName)
        {
            var instance = new PlayerMatch
            {
                PK = CreatePK(playerId.ToString()),
                SK = CreateSK(matchId.ToString()),
                MatchId = matchId,
                PlayerId = playerId,
                OpponentId = opponentId,
                PlayerName = playerName,
                OpponentName = opponentName
            };

            return instance;
        }

        private void Update(int setsWonByThePlayer, int setsWonByTheOpponent)
        {
            SetsWonByThePlayer = setsWonByThePlayer;
            SetsWonByTheOpponent = setsWonByTheOpponent;

            IsWin = setsWonByThePlayer > setsWonByTheOpponent;
        }

        public static string CreatePK(string playerId)
        {
            return $"{Constants.PlayerPrefix}#{playerId}";
        }

        public static string CreateSK(string matchId)
        {
            return $"{Constants.MatchPrefix}#{matchId}";
        }
    }
}
