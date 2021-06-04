using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TTT.DomainModel;
using TTT.DomainModel.Entities;

namespace TTT.Seasons.Repository
{
    public class SeasonRepository : ISeasonRepository
    {
        private readonly IDynamoDBContext _dbContext;

        public SeasonRepository(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Season>> ListSeasonsAsync()
        {
            var seasonsAsyncSearch = _dbContext.ScanAsync<Season>(new List<ScanCondition>
            {
                new ScanCondition("SK", ScanOperator.BeginsWith, $"{Constants.SeasonDataPrefix}#")
            });

            return seasonsAsyncSearch.GetRemainingAsync();
        }

        public Task<List<SeasonPlayer>> ListSeasonPlayersAsync(string seasonId)
        {
            var seasonsAsyncSearch = _dbContext.QueryAsync<SeasonPlayer>(
                SeasonPlayer.CreatePK(seasonId),
                QueryOperator.BeginsWith,
                new[] { $"{Constants.PlayerPrefix}#" }
            );

            return seasonsAsyncSearch.GetRemainingAsync();
        }

        public Task<Season> LoadSeasonAsync(string seasonId)
        {
            return _dbContext.LoadAsync<Season>(Season.CreatePK(seasonId), Season.CreateSK(seasonId));
        }

        public Task<List<SeasonFixture>> LoadFixturesAsync(string seasonId)
        {
            var seasonsAsyncSearch = _dbContext.QueryAsync<SeasonFixture>(
                SeasonFixture.CreatePK(seasonId),
                QueryOperator.BeginsWith,
                new[] { $"{Constants.FixturePrefix}#" }
            );

            return seasonsAsyncSearch.GetRemainingAsync();
        }

        public async Task<SeasonFixture> LoadFixtureAsync(string seasonId, string fixtureId)
        {
            var fixturesAsyncSearch = _dbContext.QueryAsync<SeasonFixture>(
                SeasonFixture.CreatePK(seasonId),
                QueryOperator.Equal,
                new[] { SeasonFixture.CreateSK(fixtureId) }
            );

            var fixture = (await fixturesAsyncSearch.GetRemainingAsync()).SingleOrDefault();

            return fixture;
        }

        public Task SaveAsync(Season season)
        {
            return _dbContext.SaveAsync(season);
        }

        public Task SaveAsync(SeasonFixture fixture)
        {
            return _dbContext.SaveAsync(fixture);
        }

        public Task SaveAsync(SeasonPlayer player)
        {
            return _dbContext.SaveAsync(player);
        }
    }
}

