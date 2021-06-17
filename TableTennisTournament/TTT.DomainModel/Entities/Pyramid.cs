using System;
using System.Collections.Generic;
using TTT.DomainModel.Enums;

namespace TTT.DomainModel.Entities
{
    public class Pyramid
    {
        public Node Root { get; set; }
        public PyramidType Type { get; set; }

        public static Pyramid CreatePyramid(List<Tuple<FixturePlayer, FixturePlayer>> combatants, PyramidType type)
        {
            var initialNumberOfMatches = combatants.Count;
            for (var i = 0; i < initialNumberOfMatches - 1; i++)
            {
                combatants.Insert(0, new Tuple<FixturePlayer, FixturePlayer>(null, null));
            }

            var instance = new Pyramid
            {
                Root = BuildPyramid(combatants),
                Type = type
            };

            return instance;
        }

        private static Node BuildPyramid(IReadOnlyList<Tuple<FixturePlayer, FixturePlayer>> combatants, int index = 0,
            int depth = 0, bool isLeft = false)
        {
            if (index >= combatants.Count)
            {
                return null;
            }

            var currentNode = new Node(combatants[index])
            {
                Depth = depth,
                IsLeft = isLeft,
                Left = BuildPyramid(combatants, 2 * index + 1, depth + 1, true),
                Right = BuildPyramid(combatants, 2 * index + 2, depth + 1)
            };

            return currentNode;
        }

        public Node FindMatchById(Guid matchId)
        {
            return FindMatchById(matchId, Root);
        }

        private static Node FindMatchById(Guid matchId, Node currentNode)
        {
            if (currentNode == null) return null;

            if (currentNode.MatchId == matchId)
            {
                return currentNode;
            }

            var foundNode = FindMatchById(matchId, currentNode.Left);

            return foundNode ?? FindMatchById(matchId, currentNode.Right);
        }

        public Node FindMatchByPlayers(Guid playerOneId, Guid playerTwoId)
        {
            return FindMatchByPlayers(playerOneId, playerTwoId, Root);
        }

        private static Node FindMatchByPlayers(Guid playerOneId, Guid playerTwoId, Node currentNode)
        {
            if (currentNode == null) return null;

            if (currentNode.PlayerOneStats?.PlayerId == playerOneId && currentNode.PlayerTwoStats?.PlayerId == playerTwoId ||
                currentNode.PlayerTwoStats?.PlayerId == playerOneId && currentNode.PlayerOneStats?.PlayerId == playerTwoId)
            {
                return currentNode;
            }

            var foundNode = FindMatchByPlayers(playerOneId, playerTwoId, currentNode.Left);

            if (foundNode != null)
            {
                return foundNode;
            }

            return FindMatchByPlayers(playerOneId, playerTwoId, currentNode.Right);
        }
    }

    public class Node
    {
        public Guid MatchId { get; set; }
        public int Depth { get; set; }

        public PlayerMatchStats PlayerOneStats { get; set; }
        public PlayerMatchStats PlayerTwoStats { get; set; }

        public Node Left { get; set; }
        public Node Right { get; set; }

        public bool IsLeft { get; set; }
        public bool IsRight => !IsLeft;

        public Node(Tuple<FixturePlayer, FixturePlayer> combatants)
        {
            MatchId = new Guid();

            var (playerOne, playerTwo) = combatants;
            if (playerOne == null || playerTwo == null) return;

            PlayerOneStats = new PlayerMatchStats { PlayerId = playerOne.PlayerId, PlayerName = playerOne.Name };
            PlayerTwoStats = new PlayerMatchStats { PlayerId = playerTwo.PlayerId, PlayerName = playerTwo.Name };
        }
    }
}
