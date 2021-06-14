using System.Collections.Generic;
using System.Threading.Tasks;
using TTT.DomainModel.Entities;

namespace TTT.Players.Repository
{
    public interface IPlayerRepository
    {
        Task<List<Player>> ListAsync();
        Task<Player> LoadAsync(string partitionKey, string sortKey);
        Task SaveAsync(Player player);
        Task SaveAsync(PlayerMatch playerMatch);
        Task DeleteAsync(Player player);
    }
}
