namespace Hermes.Persistence
{
    public interface IUserNamePersistenceAudit
    {
        string ModifiedBy { get; set; }
        string CreatedBy { get; set; }
    }
}