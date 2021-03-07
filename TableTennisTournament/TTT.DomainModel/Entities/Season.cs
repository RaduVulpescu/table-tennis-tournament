using System;
using System.Collections.Generic;

namespace TTT.DomainModel.Entities
{
    public class Season : IEntity
    {
        public string PK { get; set; }
        public int Number { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Fixture> Fixtures { get; set; }
        public List<Guid> PlayersIds { get; set; }

        public static Season Create(int number)
        {
            var instance = new Season
            {
                PK = $"SEASON#{Guid.NewGuid()}",
                Number =  number,
                Fixtures = new List<Fixture>(),
                PlayersIds = new List<Guid>()
            };

            return instance;
        }
    }
}
