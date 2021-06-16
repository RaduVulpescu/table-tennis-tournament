using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FunctionCommon;
using Microsoft.Extensions.DependencyInjection;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;
using TTT.Seasons.Repository;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace StartFixtureFunction
{
    public class Function : BaseFunction
    {
        private readonly ISeasonRepository _seasonRepository;

        public Function()
        {
            _seasonRepository = ServiceProvider.GetService<ISeasonRepository>();
        }

        public Function(ISeasonRepository seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var seasonId = request.PathParameters["seasonId"];
            var fixtureId = request.PathParameters["fixtureId"];
            var fixture = await _seasonRepository.LoadFixtureAsync(seasonId, fixtureId);

            if (fixture is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = $"Fixture with id {fixtureId} Not Found",
                    StatusCode = (int)HttpStatusCode.NotFound
                };
            }

            fixture.State = FixtureState.GroupsStage;
            fixture.Players = fixture.Players.OrderByDescending(p => p.Quality.GetValueOrDefault(0d)).ToList();

            var playersCount = fixture.Players.Count;
            if (playersCount < 9)
            {
                CreateOneGroupMatches(fixture);
            }
            else if (playersCount < 16)
            {
                CreateTwoGroupsMatches(fixture);
            }
            else
            {
                CreateFourGroupsMatches(fixture);
            }

            await _seasonRepository.SaveAsync(fixture);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }

        public void CreateOneGroupMatches(SeasonFixture fixture)
        {
            var fixturePlayers = fixture.Players.OrderByDescending(p => p.Quality.GetValueOrDefault(0)).ToArray();

            FillGroupWithPlayers(fixture, fixturePlayers, Group.A);
        }

        private void CreateTwoGroupsMatches(SeasonFixture fixture)
        {
            var fixturePlayers = fixture.Players.OrderByDescending(p => p.Quality.GetValueOrDefault(0)).ToArray();

            var groupAPlayers = fixturePlayers.Where((_, index) => index % 4 == 0 ^ index % 4 == 3).ToArray();
            var groupBPlayers = fixturePlayers.Where((_, index) => index % 4 == 1 ^ index % 4 == 2).ToArray();

            FillGroupWithPlayers(fixture, groupAPlayers, Group.A);
            FillGroupWithPlayers(fixture, groupBPlayers, Group.B);
        }

        private void CreateFourGroupsMatches(SeasonFixture fixture)
        {
            var fixturePlayers = fixture.Players.OrderByDescending(p => p.Quality.GetValueOrDefault(0)).ToArray();

            var groupAPlayers = new[] { fixturePlayers[0], fixturePlayers[7], fixturePlayers[8], fixturePlayers[15] };
            var groupBPlayers = new[] { fixturePlayers[1], fixturePlayers[6], fixturePlayers[9], fixturePlayers[14] };
            var groupCPlayers = new[] { fixturePlayers[2], fixturePlayers[5], fixturePlayers[10], fixturePlayers[13] };
            var groupDPlayers = new[] { fixturePlayers[3], fixturePlayers[4], fixturePlayers[11], fixturePlayers[12] };

            FillGroupWithPlayers(fixture, groupAPlayers, Group.A);
            FillGroupWithPlayers(fixture, groupBPlayers, Group.B);
            FillGroupWithPlayers(fixture, groupCPlayers, Group.C);
            FillGroupWithPlayers(fixture, groupDPlayers, Group.D);
        }

        private static void FillGroupWithPlayers(SeasonFixture fixture, IReadOnlyList<FixturePlayer> fixturePlayers, Group group)
        {
            for (var i = 0; i < fixturePlayers.Count; i++)
            {
                for (var j = i + 1; j < fixturePlayers.Count; j++)
                {
                    var matchId = Guid.NewGuid();

                    var playerOne = fixturePlayers[i];
                    var playerTwo = fixturePlayers[j];

                    fixture.GroupMatches.Add(new GroupMatch
                    {
                        MatchId = matchId,
                        Group = group,
                        PlayerOneStats = new PlayerMatchStats
                        {
                            PlayerId = playerOne.PlayerId,
                            PlayerName = playerOne.Name
                        },
                        PlayerTwoStats = new PlayerMatchStats
                        {
                            PlayerId = playerTwo.PlayerId,
                            PlayerName = playerTwo.Name
                        }
                    });
                }
            }
        }
    }
}
