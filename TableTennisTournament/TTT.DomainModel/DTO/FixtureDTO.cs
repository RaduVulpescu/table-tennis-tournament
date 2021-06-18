using System;
using System.Collections.Generic;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.DTO
{
    public struct FixtureGetDTO
    {
        public Guid SeasonId { get; set; }
        public Guid FixtureId { get; set; }
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public double QualityAverage { get; set; }
        public FixtureState State { get; set; }
        public FixtureType Type { get; set; }
        public List<FixturePlayer> Players { get; set; }
        public List<GroupMatch> GroupMatches { get; set; }
        public IEnumerable<FlattenPyramidDTO> Pyramids { get; set; }
        public IEnumerable<FixturePlayerRank> Ranking { get; set; }
    }

    public struct FixturePostDTO
    {
        public DateTime Date { get; set; }
        public string Location { get; set; }
    }

    public struct FixturePutDTO
    {
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public List<FixturePlayer> Players { get; set; }
    }

    public struct MatchPutDTO
    {
        public int SetsWonByPlayerOne { get; set; }
        public int SetsWonByPlayerTwo { get; set; }
    }

    public struct FlattenPyramidDTO
    {
        public PyramidType Type { get; set; }
        public List<MatchDTO> Matches { get; set; }
    }

    public struct MatchDTO
    {
        public Guid MatchId { get; set; }
        public PlayerMatchStats PlayerOneStats { get; set; }
        public PlayerMatchStats PlayerTwoStats { get; set; }
    }
}
