using System.Collections.Generic;
using System.Threading.Tasks;
using TTT.DomainModel.Entities;

namespace TTT.Seasons.Repository
{
    public interface ISeasonRepository
    {
        public Task<List<Season>> ListSeasonsAsync();
        public Task<List<SeasonPlayer>> ListSeasonPlayersAsync(string seasonId);
        public Task<Season> LoadSeasonAsync(string seasonId);
        public Task<List<SeasonFixture>> LoadFixturesAsync(string seasonId);
        public Task SaveAsync(Season season);
        public Task SaveAsync(SeasonFixture fixture);
        public Task SaveAsync(SeasonPlayer player);
    }
}
