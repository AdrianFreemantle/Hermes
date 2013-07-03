using System;

namespace Hermes.Transports.SqlServer
{
    public interface IDequeueSqlMessage
    {
        void Dequeue(Func<EnvelopeMessage, TransactionalSqlConnection, bool> tryProcessMessage);
    }
}