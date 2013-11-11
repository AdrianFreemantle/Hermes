namespace Hermes.EntityFramework.Queries
{
    public interface ISqlCommand
    {
        void SqlCommand(string sqlQuery, params object[] parameters);
    }
}