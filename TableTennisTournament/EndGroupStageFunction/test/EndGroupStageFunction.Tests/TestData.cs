using System;
using System.Collections.Generic;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;

namespace EndGroupStageFunction.Tests
{
    public static class TestData
    {
        public static readonly Guid Player1Guid = Guid.Parse("E738582B-74CB-46B7-9EAB-6ADDEA6E48AC");
        public static readonly Guid Player2Guid = Guid.Parse("B0FB4E6D-A39D-436B-9F0A-AEB703681998");
        public static readonly Guid Player3Guid = Guid.Parse("E6FCD455-EAE1-44BB-9552-202F147AAE4F");
        public static readonly Guid Player4Guid = Guid.Parse("16B42531-16EB-48E9-A709-E4A63F699007");
        public static readonly Guid Player5Guid = Guid.Parse("23292D00-4C56-4558-8F19-7F1273A5DAC4");
        public static readonly Guid Player6Guid = Guid.Parse("936B6F49-D438-4F74-ADB6-8A5F1009D5FD");
        public static readonly Guid Player7Guid = Guid.Parse("86758681-19CA-43D4-9F5B-C72D78EFCE06");
        public static readonly Guid Player8Guid = Guid.Parse("AB513F8B-0784-4247-9AF1-118E0F943D4A");
        public static readonly Guid Player9Guid = Guid.Parse("38DEDE12-6EC2-46B4-9AD0-A8D5C5A72905");
        public static readonly Guid Player10Guid = Guid.Parse("98817C7A-E885-4484-9D51-06D82A66C69B");
        public static readonly Guid Player11Guid = Guid.Parse("E8F10F60-63E5-4B85-954B-B58CEE2DA37F");
        public static readonly Guid Player12Guid = Guid.Parse("CEE6370C-7BEA-49F8-B39B-4F856764DC6F");
        public static readonly Guid Player13Guid = Guid.Parse("0A356BF3-E3E5-4EBD-AD11-88EE9832D854");
        public static readonly Guid Player14Guid = Guid.Parse("E9C9714D-64D7-4055-80F9-8ABA199EFECE");
        public static readonly Guid Player15Guid = Guid.Parse("78B4DFC1-C233-46C6-8794-D5BCD12DDF0F");
        public static readonly Guid Player16Guid = Guid.Parse("2D1EB73D-B48C-4D25-8ECC-6CA9C966C3C2");

        public static SeasonFixture CreateOrderedFourPlayersGroupFixture()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 75.24,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0)
                }
            };
        }

        public static SeasonFixture CreateTwoEqualVictoryPlayers()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid }
                },
                QualityAverage = 68.86,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player3Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player4Guid, Player5Guid, 0, 3)
                }
            };
        }

        public static SeasonFixture CreateThreeManPartialBarrageFixtureWithATie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 77.77,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3)
                }
            };
        }

        public static SeasonFixture CreateThreeManPartialBarrageFixtureWithNoTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 70.21,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 1, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3)
                }
            };
        }

        public static SeasonFixture CreateThreeManCompleteBarrageFixtureWithTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 70.94,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 1),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 1),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 2, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 2),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3)
                }
            };
        }

        public static SeasonFixture CreateThreeManCompleteBarrageFixtureWithNoTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 74.57,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 1),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 2),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 2, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 2),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 2, 3)
                }
            };
        }

        public static SeasonFixture CreateThreeManPerfectTie()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid }
                },
                QualityAverage = 70.21,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 0, 3)
                }
            };
        }

        public static SeasonFixture CreateTwoOrderedFourPlayersGroupFixture()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid },
                    new FixturePlayer { PlayerId = Player6Guid },
                    new FixturePlayer { PlayerId = Player7Guid },
                    new FixturePlayer { PlayerId = Player8Guid },
                    new FixturePlayer { PlayerId = Player9Guid }
                },
                QualityAverage = 75.24,
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 0, 3),
                    CreateGroupMatch(Group.A, Player2Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player3Guid, Player5Guid, 3, 0),

                    CreateGroupMatch(Group.A, Player4Guid, Player5Guid, 0, 3),

                    CreateGroupMatch(Group.B, Player6Guid, Player7Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player6Guid, Player8Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player6Guid, Player9Guid, 3, 0),

                    CreateGroupMatch(Group.B, Player7Guid, Player8Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player7Guid, Player8Guid, 3, 0),

                    CreateGroupMatch(Group.B, Player8Guid, Player9Guid, 3, 0)
                }
            };
        }

        public static SeasonFixture CreateFourGroupsWithFourOrderedPlayersFixture()
        {
            return new SeasonFixture
            {
                Players = new List<FixturePlayer>
                {
                    new FixturePlayer { PlayerId = Player1Guid },
                    new FixturePlayer { PlayerId = Player2Guid },
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid },
                    new FixturePlayer { PlayerId = Player6Guid },
                    new FixturePlayer { PlayerId = Player7Guid },
                    new FixturePlayer { PlayerId = Player8Guid },
                    new FixturePlayer { PlayerId = Player9Guid },
                    new FixturePlayer { PlayerId = Player10Guid },
                    new FixturePlayer { PlayerId = Player11Guid },
                    new FixturePlayer { PlayerId = Player12Guid },
                    new FixturePlayer { PlayerId = Player13Guid },
                    new FixturePlayer { PlayerId = Player14Guid },
                    new FixturePlayer { PlayerId = Player15Guid },
                    new FixturePlayer { PlayerId = Player16Guid }
                },
                GroupMatches = new List<GroupMatch>
                {
                    CreateGroupMatch(Group.A, Player1Guid, Player2Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player1Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player3Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player2Guid, Player4Guid, 3, 0),
                    CreateGroupMatch(Group.A, Player3Guid, Player4Guid, 3, 0),

                    CreateGroupMatch(Group.B, Player5Guid, Player6Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player5Guid, Player7Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player5Guid, Player8Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player6Guid, Player7Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player6Guid, Player8Guid, 3, 0),
                    CreateGroupMatch(Group.B, Player7Guid, Player8Guid, 3, 0),

                    CreateGroupMatch(Group.C, Player9Guid, Player10Guid, 3, 0),
                    CreateGroupMatch(Group.C, Player9Guid, Player11Guid, 3, 0),
                    CreateGroupMatch(Group.C, Player9Guid, Player12Guid, 3, 0),
                    CreateGroupMatch(Group.C, Player10Guid, Player11Guid, 3, 0),
                    CreateGroupMatch(Group.C, Player10Guid, Player12Guid, 3, 0),
                    CreateGroupMatch(Group.C, Player11Guid, Player12Guid, 3, 0),

                    CreateGroupMatch(Group.D, Player13Guid, Player14Guid, 3, 0),
                    CreateGroupMatch(Group.D, Player13Guid, Player15Guid, 3, 0),
                    CreateGroupMatch(Group.D, Player13Guid, Player16Guid, 3, 0),
                    CreateGroupMatch(Group.D, Player14Guid, Player15Guid, 3, 0),
                    CreateGroupMatch(Group.D, Player14Guid, Player16Guid, 3, 0),
                    CreateGroupMatch(Group.D, Player15Guid, Player16Guid, 3, 0)
                }
            };
        }

        public static GroupMatch CreateGroupMatch(Group group, Guid playerOneGuid, Guid playerTwoGuid, int playerOneScore, int playerTwoScore) => new GroupMatch
        {
            Group = group,
            PlayerOneStats = new PlayerMatchStats { PlayerId = playerOneGuid, SetsWon = playerOneScore },
            PlayerTwoStats = new PlayerMatchStats { PlayerId = playerTwoGuid, SetsWon = playerTwoScore }
        };
    }
}
