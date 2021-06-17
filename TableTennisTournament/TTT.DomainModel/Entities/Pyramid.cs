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

        public Node FindMatchById(Guid matchId)
        {
            return FindMatchById(matchId, Root);
        }

        public Node FindMatchByPlayers(Guid playerOneId, Guid playerTwoId)
        {
            return FindMatchByPlayers(playerOneId, playerTwoId, Root);
        }

        public List<Node> FindMatchesOnLevel(int level)
        {
            var matchesOnLevel = new List<Node>();
            FindMatchesOnLevel(level, matchesOnLevel, Root);
            return matchesOnLevel;
        }

        public List<Node> ToList()
        {
            var list = new List<Node>();
            return ToList(list, Root);
        }

        private static Node BuildPyramid(IReadOnlyList<Tuple<FixturePlayer, FixturePlayer>> combatants, int index = 0,
            int level = 0, bool isLeft = false)
        {
            if (index >= combatants.Count)
            {
                return null;
            }

            var currentNode = new Node(combatants[index])
            {
                Level = level,
                IsLeft = isLeft,
                Left = BuildPyramid(combatants, 2 * index + 1, level + 1, true),
                Right = BuildPyramid(combatants, 2 * index + 2, level + 1)
            };

            return currentNode;
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

        private static Node FindMatchByPlayers(Guid playerOneId, Guid playerTwoId, Node currentNode)
        {
            if (currentNode == null) return null;

            if (currentNode.PlayerOneStats?.PlayerId == playerOneId && currentNode.PlayerTwoStats?.PlayerId == playerTwoId ||
                currentNode.PlayerTwoStats?.PlayerId == playerOneId && currentNode.PlayerOneStats?.PlayerId == playerTwoId)
            {
                return currentNode;
            }

            var foundNode = FindMatchByPlayers(playerOneId, playerTwoId, currentNode.Left);

            return foundNode ?? FindMatchByPlayers(playerOneId, playerTwoId, currentNode.Right);
        }

        private static void FindMatchesOnLevel(int level, ICollection<Node> nodesOnLevel, Node currentNode)
        {
            if (currentNode == null) return;

            if (currentNode.Level == level)
            {
                nodesOnLevel.Add(currentNode);
                return;
            }

            FindMatchesOnLevel(level, nodesOnLevel, currentNode.Left);
            FindMatchesOnLevel(level, nodesOnLevel, currentNode.Right);
        }

        public List<Node> ToList(List<Node> list, Node currentNode)
        {
            if (currentNode == null) return list;

            ToList(list, currentNode.Left);
            ToList(list, currentNode.Right);

            var insertIndex = list.FindLastIndex(x => x.Level == currentNode.Level);
            if (insertIndex == -1)
            {
                list.Add(currentNode);
            }
            else
            {
                list.Insert(insertIndex + 1, currentNode);
            }

            return list;
        }
    }

    public class Node
    {
        public Guid MatchId { get; set; }
        public int Level { get; set; }

        public PlayerMatchStats PlayerOneStats { get; set; }
        public PlayerMatchStats PlayerTwoStats { get; set; }
        public bool IsFinished { get; set; }

        public Node Left { get; set; }
        public Node Right { get; set; }

        public bool IsLeft { get; set; }

        public Node() { }

        public Node(Tuple<FixturePlayer, FixturePlayer> combatants)
        {
            MatchId = new Guid();

            var (playerOne, playerTwo) = combatants;
            if (playerOne == null || playerTwo == null) return;

            PlayerOneStats = new PlayerMatchStats { PlayerId = playerOne.PlayerId, PlayerName = playerOne.Name };
            PlayerTwoStats = new PlayerMatchStats { PlayerId = playerTwo.PlayerId, PlayerName = playerTwo.Name };
        }

        public Node FindParent(Pyramid pyramid)
        {
            return FindParent(MatchId, pyramid.Root);
        }

        public Node FindSibling(Pyramid pyramid)
        {
            var parent = FindParent(pyramid);
            return IsLeft ? parent?.Right : parent?.Left;
        }

        public PlayerMatchStats GetWinner()
        {
            if (!PlayerOneStats.SetsWon.HasValue || !PlayerTwoStats.SetsWon.HasValue) return null;

            return PlayerOneStats.SetsWon.Value > PlayerTwoStats.SetsWon.Value ? PlayerOneStats : PlayerTwoStats;
        }

        public PlayerMatchStats GetLoser()
        {
            if (!PlayerOneStats.SetsWon.HasValue || !PlayerTwoStats.SetsWon.HasValue) return null;

            return PlayerOneStats.SetsWon.Value < PlayerTwoStats.SetsWon.Value ? PlayerOneStats : PlayerTwoStats;
        }

        private Node FindParent(Guid matchId, Node currentNode)
        {
            if (currentNode.Level >= Level || currentNode.Left == null || currentNode.Right == null) return null;

            if (currentNode.Left.MatchId == matchId || currentNode.Right.MatchId == matchId)
            {
                return currentNode;
            }

            var foundNode = FindParent(matchId, currentNode.Left);

            return foundNode ?? FindParent(matchId, currentNode.Right);
        }
    }
}
