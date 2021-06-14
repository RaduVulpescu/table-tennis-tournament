using System;

namespace TTT.DomainModel.Entities
{
    public class PlayerMatchStats
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int? SetsWon { get; set; }
    }
}
