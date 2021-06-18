using System.Collections.Generic;
using System.Linq;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel
{
    public static class Mapper
    {
        public static SeasonGetDTO ToDTO(this Season season)
        {
            return new SeasonGetDTO
            {
                SeasonId = season.SeasonId,
                Number = season.Number,
                StartDate = season.StartDate,
                EndDate = season.EndDate
            };
        }

        public static FixtureGetDTO ToDTO(this SeasonFixture fixture)
        {
            var canBeEnded = fixture.State == FixtureState.GroupsStage && fixture.Pyramids == null &&
                fixture.GroupMatches.All(x => x.PlayerOneStats.SetsWon.HasValue && x.PlayerTwoStats.SetsWon.HasValue);

            var flattenPyramids = fixture.Pyramids?.Select(p => p.PyramidToDTO()).ToArray();

            canBeEnded = flattenPyramids?.Aggregate(canBeEnded, (current, pyramid) => current && pyramid.IsComplete) ?? canBeEnded;

            return new FixtureGetDTO
            {
                SeasonId = fixture.SeasonId,
                FixtureId = fixture.FixtureId,
                Date = fixture.Date,
                Location = fixture.Location,
                QualityAverage = fixture.QualityAverage,
                State = fixture.State,
                Type = fixture.Type,
                CanBeEnded = canBeEnded,
                Players = fixture.Players,
                GroupMatches = fixture.GroupMatches,
                Pyramids = flattenPyramids,
                Ranking = fixture.Ranking?.OrderBy(r => r.Rank)
            };
        }

        public static FlattenPyramidDTO PyramidToDTO(this Pyramid pyramid)
        {
            var instance = new FlattenPyramidDTO
            {
                Type = pyramid.Type,
                Matches = new List<MatchDTO>()
            };

            var matches = pyramid.ToList();
            var flattenMatches = matches.Where(x => x.PlayerOneStats != null && x.PlayerTwoStats != null).ToArray();

            foreach (var match in flattenMatches)
            {
                instance.Matches.Add(new MatchDTO
                {
                    MatchId = match.MatchId,
                    PlayerOneStats = match.PlayerOneStats,
                    PlayerTwoStats = match.PlayerTwoStats
                });
            }

            instance.IsComplete = matches.Count == flattenMatches.Length;

            return instance;
        }
    }
}
