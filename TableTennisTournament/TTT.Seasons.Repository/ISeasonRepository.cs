using System.Collections.Generic;
using System.Threading.Tasks;
using TTT.DomainModel.Entities;

namespace TTT.Seasons.Repository
{
    public interface ISeasonRepository
    {
        public Task<List<Season>> ListAsync();
        public Task<Season> LoadAsync(string partitionKey, string sortKey);
        public Task SaveAsync(Season season);
        public Task SaveAsync(SeasonFixture fixture);
    }
}
