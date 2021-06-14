using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TTT.DomainModel;
using TTT.DomainModel.Entities;

namespace TTT.Players.Repository
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly IDynamoDBContext _dbContext;

        public PlayerRepository(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Player>> ListAsync()
        {
            var playersAsyncSearch = _dbContext.ScanAsync<Player>(new List<ScanCondition>
            {
                new ScanCondition("SK", ScanOperator.BeginsWith, $"{Constants.PlayerDataPrefix}#")
            });

            return playersAsyncSearch.GetRemainingAsync();
        }

        public Task<Player> LoadAsync(string partitionKey, string sortKey)
        {
            return _dbContext.LoadAsync<Player>(partitionKey, sortKey);
        }

        public Task SaveAsync(Player player)
        {
            return _dbContext.SaveAsync(player);
        }

        public Task SaveAsync(PlayerMatch playerMatch)
        {
            return _dbContext.SaveAsync(playerMatch);
        }

        public Task DeleteAsync(Player player)
        {
            return _dbContext.DeleteAsync(player);
        }
    }
}
