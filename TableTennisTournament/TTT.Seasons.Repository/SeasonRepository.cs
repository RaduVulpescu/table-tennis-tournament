﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

        public Task<List<Season>> ListAsync()
        {
            var seasonsAsyncSearch = _dbContext.ScanAsync<Season>(new List<ScanCondition>
            {
                new ScanCondition("SK", ScanOperator.BeginsWith, "SEASON_DATA#")
            });

            return seasonsAsyncSearch.GetRemainingAsync();
        }

        public Task<Season> LoadAsync(string partitionKey, string sortKey)
        {
            return _dbContext.LoadAsync<Season>(partitionKey, sortKey);
        }


        public Task SaveAsync(Season season)
        {
            return _dbContext.SaveAsync(season);
        }
    }
}
