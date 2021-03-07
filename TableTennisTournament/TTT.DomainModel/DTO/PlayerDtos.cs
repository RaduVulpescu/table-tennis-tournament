using System.ComponentModel.DataAnnotations;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.DTO
{
    public struct PlayerDTO
    {
        [Required]
        [MaxLength(80)]
        public string Name { get; set; }

        [MaxLength(80)]
        public string City { get; set; }

        //TODO: Custom validator
        [Range(1930, 2019)]
        public int? BirthYear { get; set; }

        [Range(20, 250)]
        public int? Height { get; set; }

        [Range(30, 200)]
        public int? Weight { get; set; }

        [Range(1, 6)]
        public Levels CurrentLevel { get; set; }
    }
}
