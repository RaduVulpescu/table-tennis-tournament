namespace TTT.DomainModel.Entities
{
    public interface IDynamoItem
    {
        string PK { get; set; }
        string SK { get; set; }
    }
}
