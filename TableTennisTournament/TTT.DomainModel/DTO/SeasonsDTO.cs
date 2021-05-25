using System;

namespace TTT.DomainModel.DTO
{
    public class SeasonGetDTO
    {
        public Guid SeasonId { get; set; }
        public int Number { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SeasonsPatchDTO
    {
        public DateTime EndDate { get; set; }
    }
}
