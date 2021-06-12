using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    [DynamoDBTable("table-tennis-tournament")]
    public class SeasonFixture : IDynamoItem
    {
        [DynamoDBHashKey]
        public string PK { get; set; }
        [DynamoDBRangeKey]
        public string SK { get; set; }

        public Guid SeasonId { get; set; }
        public Guid FixtureId { get; set; }
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public double QualityAverage { get; private set; }
        public FixtureState State { get; set; }
        public FixtureType Type { get; set; }
        public List<FixturePlayer> Players { get; set; }
        public List<GroupMatch> GroupMatches { get; set; }
        public List<Pyramid> Pyramids { get; set; }
        public List<FixturePlayerRank> Ranking { get; set; }

        public static SeasonFixture Create(Guid seasonId, FixtureType type, DateTime? date = null, string location = null)
        {
            var fixtureId = Guid.NewGuid();

            var instance = new SeasonFixture
            {
                PK = CreatePK(seasonId.ToString()),
                SK = CreateSK(fixtureId.ToString()),
                SeasonId = seasonId,
                FixtureId = fixtureId,
                QualityAverage = 0d,
                State = FixtureState.Upcoming,
                Type = type,
                Pyramids = new List<Pyramid>(),
                GroupMatches = new List<GroupMatch>(),
                Ranking = new List<FixturePlayerRank>()
            };

            instance.Update(date, location, new List<FixturePlayer>());

            return instance;
        }

        public void Update(DateTime? date, string location, List<FixturePlayer> players)
        {
            Date = date;
            Location = location;
            Players = players;
            QualityAverage = players.Select(x => x.Quality.GetValueOrDefault(0d)).DefaultIfEmpty(0d).Average();
        }

        public static string CreatePK(string seasonId)
        {
            return $"{Constants.SeasonPrefix}#{seasonId}";
        }

        public static string CreateSK(string fixtureId)
        {
            return $"{Constants.FixturePrefix}#{fixtureId}";
        }
    }

    public class FixturePlayer
    {
        public Guid PlayerId { get; set; }
        public string Name { get; set; }
        public double? Quality { get; set; }
    }

    public class GroupMatch
    {
        public Group Group { get; set; }
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }
        public int? SetsWonByPlayerOne { get; set; }
        public int? SetsWonByPlayerTwo { get; set; }
    }

    public class FixturePlayerRank
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int? Rank { get; set; }
        public double? Score { get; set; }
    }

    //public List<FixtureMatch> Matches { get; set; }
    //public class FixtureMatch
    //{
    //    public MatchType Type { get; set; }
    //    public Guid PlayerOneId { get; set; }
    //    public string PlayerOneName { get; set; }
    //    public Guid PlayerTwoId { get; set; }
    //    public string PlayerTwoName { get; set; }
    //    public int PlayerOneSetsWon { get; set; }
    //    public int PlayerTwoSetsWon { get; set; }
    //}
}
