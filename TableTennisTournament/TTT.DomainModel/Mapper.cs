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
    }
}
