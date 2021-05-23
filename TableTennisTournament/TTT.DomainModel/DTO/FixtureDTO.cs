using System;
using System.ComponentModel.DataAnnotations;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.DTO
{
    public struct FixturePutDTO
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Location { get; set; }
    }

    public struct FixtureStateDTO
    {
        [Required]
        public FixtureState FixtureState { get; set; }
    }

    public struct FixturePlayerDTO
    {
        [Required]
        public Guid PlayerId { get; set; }
    }
}
