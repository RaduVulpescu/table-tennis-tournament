using System;

namespace TTT.DomainModel.Entities
{
    public class Match
    {
        public int SetsWonByPlayerOne { get; set; }
        public int SetsWonByPlayerTwo { get; set; }
        public Guid PlayerOneId { get; set; }
        public Guid PlayerTwoId { get; set; }
    }
}
