using System.Collections.Generic;
using System.Linq;
using TTT.DomainModel.DTO;
using TTT.DomainModel.Entities;

namespace TTT.Seasons.Repository
{
    public static class SeasonMapper
    {
        public static FlattenPyramidDTO PyramidToDTO(Pyramid pyramid)
        {
            var instance = new FlattenPyramidDTO
            {
                Type = pyramid.Type,
                Matches = new List<MatchDTO>()
            };

            var matches = pyramid.ToList().Where(x => x.PlayerOneStats != null && x.PlayerTwoStats != null);
            foreach (var match in matches)
            {
                instance.Matches.Add(new MatchDTO
                {
                    MatchId = match.MatchId,
                    PlayerOneStats = match.PlayerOneStats,
                    PlayerTwoStats = match.PlayerTwoStats
                });
            }

            return instance;
        }
    }
}
