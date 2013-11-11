namespace Hermes.EntityFramework
{
    public interface ILookupTable
    {
        int Id { get; }
        string Description { get; set; }
    }
}