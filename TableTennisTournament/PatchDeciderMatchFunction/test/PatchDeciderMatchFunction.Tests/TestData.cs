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

        public static readonly Guid MatchGuid1 = Guid.Parse("4B6E39B9-9C82-4D84-9288-9307834F9126");

        public static SeasonFixture CreateFixtureWithZeroDepthPyramid()
        {
            var playerOne = new FixturePlayer { PlayerId = Player1Guid };
            var playerTwo = new FixturePlayer { PlayerId = Player2Guid };

            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    playerOne,
                    playerTwo,
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid },
                    new FixturePlayer { PlayerId = Player6Guid },
                    new FixturePlayer { PlayerId = Player7Guid },
                    new FixturePlayer { PlayerId = Player8Guid },
                    new FixturePlayer { PlayerId = Player9Guid },
                    new FixturePlayer { PlayerId = Player10Guid }
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_1_2,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(playerOne , playerTwo))
                        {
                            Depth = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        public static SeasonFixture CreateFixtureWithZeroDepthPyramidForRank7And8()
        {
            var playerOne = new FixturePlayer { PlayerId = Player1Guid };
            var playerTwo = new FixturePlayer { PlayerId = Player2Guid };

            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    playerOne,
                    playerTwo,
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid },
                    new FixturePlayer { PlayerId = Player6Guid },
                    new FixturePlayer { PlayerId = Player7Guid },
                    new FixturePlayer { PlayerId = Player8Guid },
                    new FixturePlayer { PlayerId = Player9Guid }
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_7_8,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(playerOne , playerTwo))
                        {
                            Depth = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }

        public static SeasonFixture CreateFixtureWithZeroDepthMatchForRank5And6()
        {
            var playerOne = new FixturePlayer { PlayerId = Player1Guid };
            var playerTwo = new FixturePlayer { PlayerId = Player2Guid };

            return new SeasonFixture
            {
                QualityAverage = 70,
                Players = new List<FixturePlayer>
                {
                    playerOne,
                    playerTwo,
                    new FixturePlayer { PlayerId = Player3Guid },
                    new FixturePlayer { PlayerId = Player4Guid },
                    new FixturePlayer { PlayerId = Player5Guid },
                    new FixturePlayer { PlayerId = Player6Guid },
                    new FixturePlayer { PlayerId = Player7Guid },
                    new FixturePlayer { PlayerId = Player8Guid }
                },
                Pyramids = new List<Pyramid>
                {
                    new Pyramid
                    {
                        Type = PyramidType.Ranks_5_6,
                        Root = new Node(new Tuple<FixturePlayer, FixturePlayer>(playerOne , playerTwo))
                        {
                            Depth = 0,
                            MatchId = MatchGuid1
                        }
                    }
                },
                Ranking = new List<FixturePlayerRank>()
            };
        }
    }
}
