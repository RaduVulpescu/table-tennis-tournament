using TTT.DomainModel.Enums;

namespace TTT.DomainModel.DTO
{
    public class PlayerDTO
    {
        public string Name { get; set; }
        public string City { get; set; }
        public int? BirthYear { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public Level CurrentLevel { get; set; }
    }
}
