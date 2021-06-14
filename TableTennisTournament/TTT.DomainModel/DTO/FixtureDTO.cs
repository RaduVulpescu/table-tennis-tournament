using System;
using System.Collections.Generic;
using TTT.DomainModel.Entities;

namespace TTT.DomainModel.DTO
{
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
}
