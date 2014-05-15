namespace Hermes.EntityFramework
{
    public interface IPersistenceAudit : ITimestampPersistenceAudit, IUserNamePersistenceAudit
    {
    }
}