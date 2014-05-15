namespace Hermes.EntityFramework
{
    public interface IUserNamePersistenceAudit
    {
        string ModifiedBy { get; set; }
        string CreatedBy { get; set; }
    }
}