namespace TTT.DomainModel.Entities
{
    public class Pyramid
    {
        public Node Root { get; set; }
        public int Depth { get; set; }
        public int BestPlace { get; set; }
    }

    public class Node
    {
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }
        public int SetsWonByPlayerOne { get; set; }
        public int SetsWonByPlayerTwo { get; set; }

        public Node Left { get; set; }
        public Node Right { get; set; }
    }
}
