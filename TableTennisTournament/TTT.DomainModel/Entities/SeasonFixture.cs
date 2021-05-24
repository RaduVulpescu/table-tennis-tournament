using System;
using System.Collections.Generic;
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
        public int? Number { get; set; }
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public double QualityAverage { get; set; }
        public FixtureState State { get; set; }
        public FixtureType Type { get; set; }
        public List<GroupMatch> GroupMatches { get; set; }
        public List<Pyramid> Pyramids { get; set; }
        public List<FixturePlayerRank> Ranking { get; set; }

        public static SeasonFixture Create(Guid seasonId, FixtureType type, int? number = null, DateTime? date = null, string location = null)
        {
            var fixtureId = Guid.NewGuid();

            var instance = new SeasonFixture
            {
                PK = CreatePK(seasonId.ToString()),
                SK = CreateSK(fixtureId.ToString()),
                SeasonId = seasonId,
                FixtureId = fixtureId,
                Number = number,
                QualityAverage = 0,
                State = FixtureState.GroupsSelection,
                Type = type,
                Pyramids = new List<Pyramid>(),
                GroupMatches = new List<GroupMatch>(),
                Ranking = new List<FixturePlayerRank>()
            };

            instance.Update(date, location);

            return instance;
        }

        public void Update(DateTime? date, string location)
        {
            Date = date;
            Location = location;
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

    public class GroupMatch
    {
        public Group Group { get; set; }
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }
        public int SetsWonByPlayerOne { get; set; }
        public int SetsWonByPlayerTwo { get; set; }
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
