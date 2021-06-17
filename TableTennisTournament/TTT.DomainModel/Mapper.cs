﻿using System.Collections.Generic;
using System.Linq;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;

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
            return new FixtureGetDTO
            {
                SeasonId = fixture.SeasonId,
                FixtureId = fixture.FixtureId,
                Date = fixture.Date,
                Location = fixture.Location,
                QualityAverage = fixture.QualityAverage,
                State = fixture.State,
                Type = fixture.Type,
                Players = fixture.Players,
                GroupMatches = fixture.GroupMatches,
                Pyramids = fixture.Pyramids.Select(p => p.PyramidToDTO()),
                Ranking = fixture.Ranking
            };
        }

        public static FlattenPyramidDTO PyramidToDTO(this Pyramid pyramid)
        {
            var instance = new FlattenPyramidDTO
            {
                Type = pyramid.Type,
                Matches = new List<MatchDTO>()
            };

            var matches = pyramid.ToList().Where(x => x.PlayerOneStats != null && x.PlayerTwoStats != null);
            foreach (var match in matches)
            {
                instance.Matches.Add(new MatchDTO
                {
                    MatchId = match.MatchId,
                    PlayerOneStats = match.PlayerOneStats,
                    PlayerTwoStats = match.PlayerTwoStats
                });
            }

            return instance;
        }
    }
}
