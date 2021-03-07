using System;
using System.Collections.Generic;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    public class Fixture
    {
        public int Number { get; set; }
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public double QualityAverage { get; set; }
        public FixtureState State { get; set; }
        public FixtureType Type { get; set; }
        public List<Match> Matches { get; set; }
        public List<FixturePlayerRank> Ranking { get; set; }

        public static Fixture Create(int number, FixtureType type, DateTime? date = null, string location = null)
        {
            var instance = new Fixture
            {
                Number = number,
                QualityAverage = 0,
                State = FixtureState.GroupsSelection,
                Type = type,
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
    }

    public class FixturePlayerRank
    {
        public Guid PlayerId { get; set; }
        public int? Rank { get; set; }
        public double? Score { get; set; }
    }
}
