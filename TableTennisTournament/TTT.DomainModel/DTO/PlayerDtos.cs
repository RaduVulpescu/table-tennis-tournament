using TTT.DomainModel.Enums;

namespace TTT.DomainModel.DTO
{
    public struct PlayerDTO
    {
        public string Name { get; set; }
        public string City { get; set; }
        public int? BirthYear { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public Levels CurrentLevel { get; set; }
    }
}
