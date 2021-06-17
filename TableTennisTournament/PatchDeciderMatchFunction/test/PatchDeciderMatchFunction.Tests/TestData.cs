using System;
using System.Collections.Generic;
using TTT.DomainModel.Entities;
using TTT.DomainModel.Enums;

namespace PatchDeciderMatchFunction.Tests
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

        public static readonly FixturePlayer Player1 = new FixturePlayer { PlayerId = Player1Guid };
        public static readonly FixturePlayer Player2 = new FixturePlayer { PlayerId = Player2Guid };
        public static readonly FixturePlayer Player3 = new FixturePlayer { PlayerId = Player3Guid };
        public static readonly FixturePlayer Player4 = new FixturePlayer { PlayerId = Player4Guid };
        public static readonly FixturePlayer Player5 = new FixturePlayer { PlayerId = Player5Guid };
        public static readonly FixturePlayer Player6 = new FixturePlayer { PlayerId = Player6Guid };
        public static readonly FixturePlayer Player7 = new FixturePlayer { PlayerId = Player7Guid };
        public static readonly FixturePlayer Player8 = new FixturePlayer { PlayerId = Player8Guid };
        public static readonly FixturePlayer Player9 = new FixturePlayer { PlayerId = Player9Guid };
        public static readonly FixturePlayer Player10 = new FixturePlayer { PlayerId = Player10Guid };
        public static readonly FixturePlayer Player11 = new FixturePlayer { PlayerId = Player11Guid };
        public static readonly FixturePlayer Player12 = new FixturePlayer { PlayerId = Player12Guid };
        public static readonly FixturePlayer Player13 = new FixturePlayer { PlayerId = Player13Guid };
        public static readonly FixturePlayer Player14 = new FixturePlayer { PlayerId = Player14Guid };
        public static readonly FixturePlayer Player15 = new FixturePlayer { PlayerId = Player15Guid };
        public static readonly FixturePlayer Player16 = new FixturePlayer { PlayerId = Player16Guid };

        public static readonly Guid MatchGuid1 = Guid.Parse("4B6E39B9-9C82-4D84-9288-9307834F9126");
        public static readonly Guid MatchGuid2 = Guid.Parse("500091C3-9123-4405-93D4-E336653ED726");
        public static readonly Guid MatchGuid3 = Guid.Parse("3ACE344E-2192-4B06-89B8-26E65863F235");
        public static readonly Guid MatchGuid4 = Guid.Parse("621C52E7-0EBD-45FB-AA0A-0017FB171D5D");
        public static readonly Guid MatchGuid5 = Guid.Parse("4E45F3E7-4818-4E0F-9F09-6E59A6F5CEB2");
        public static readonly Guid MatchGuid6 = Guid.Parse("F3FC3D04-5735-4A45-97D2-A3CBFC43C536");
        public static readonly Guid MatchGuid7 = Guid.Parse("2BE210A6-016A-4F4B-A435-613B68C1028C");

        public static SeasonFixture CreateFixtureWithZeroDepthPyramid()
        {
            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    Player1,
                    Player2,
                    Player3,
                    Player4,
                    Player5,
                    Player6,
                    Player7,
                    Player8,
                    Player9,
                    Player10
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_1_2,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(Player1 , Player2))
                        {
                            Level = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        public static SeasonFixture CreateFixtureWithZeroDepthPyramidForRank7And8()
        {
            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    Player1,
                    Player2,
                    Player3,
                    Player4,
                    Player5,
                    Player6,
                    Player7,
                    Player8,
                    Player9
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_7_8,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(Player1 , Player2))
                        {
                            Level = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        public static SeasonFixture CreateFixtureWithZeroDepthMatchForRank5And6()
        {
            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    Player1,
                    Player2,
                    Player3,
                    Player4,
                    Player5,
                    Player6,
                    Player7,
                    Player8
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_5_6,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(Player1 , Player2))
                        {
                            Level = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        public static SeasonFixture CreateFixtureWith1_2Pyramid()
        {
            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    Player1,
                    Player2,
                    Player3,
                    Player4,
                    Player5,
                    Player6,
                    Player7,
                    Player8
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_1_2,
                        Root = Create1_2Pyramid()
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        private static Node Create1_2Pyramid()
        {
            var root = new Node
            {
                Level = 0,
                MatchId = MatchGuid7,
                Left = new Node
                {
                    Level = 1,
                    MatchId = MatchGuid5,
                    IsLeft = true,
                    Left = new Node
                    {
                        Level = 2,
                        MatchId = MatchGuid1,
                        PlayerOneStats = new PlayerMatchStats { PlayerId = Player1Guid, SetsWon = 3 },
                        PlayerTwoStats = new PlayerMatchStats { PlayerId = Player2Guid, SetsWon = 0 },
                        IsFinished = true,
                        IsLeft = true,
                        Left = null,
                        Right = null
                    },
                    Right = new Node(new Tuple<FixturePlayer, FixturePlayer>(Player3, Player4))
                    {
                        Level = 2,
                        MatchId = MatchGuid2,
                        IsLeft = false,
                        Left = null,
                        Right = null
                    }
                },
                Right = new Node
                {
                    Level = 1,
                    MatchId = MatchGuid6,
                    IsLeft = false,
                    Left = new Node
                    {
                        Level = 2,
                        MatchId = MatchGuid3,
                        PlayerOneStats = new PlayerMatchStats { PlayerId = Player5.PlayerId, SetsWon = 3 },
                        PlayerTwoStats = new PlayerMatchStats { PlayerId = Player6.PlayerId, SetsWon = 0 },
                        IsFinished = true,
                        IsLeft = true,
                        Left = null,
                        Right = null
                    },
                    Right = new Node
                    {
                        Level = 2,
                        MatchId = MatchGuid4,
                        PlayerOneStats = new PlayerMatchStats { PlayerId = Player7.PlayerId, SetsWon = 3 },
                        PlayerTwoStats = new PlayerMatchStats { PlayerId = Player8.PlayerId, SetsWon = 0 },
                        IsFinished = true,
                        IsLeft = false,
                        Left = null,
                        Right = null
                    }
                }
            };

            return root;
        }
    }
}
