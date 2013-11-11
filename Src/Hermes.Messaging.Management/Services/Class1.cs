using System;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace Hermes.Messaging.Management.Services
{
    public class TableSchemaService : Service
    {
        IDbConnectionFactory dbFactory = new OrmLiteConnectionFactory("Data Source=.\\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=SSPI", SqlServerDialect.Provider);

        public object Any(TableSchemaQuery request)
        {
            var connection = dbFactory.OpenDbConnection();
            return connection.Select<TableSchema>("SELECT TABLE_SCHEMA AS 'SCHEMA', TABLE_NAME AS 'TABLE' FROM information_schema.tables Where TABLE_SCHEMA = {0}", request.SchemaFilter);
        }
    }

    public class TableSchema
    {
        public string Schema { get; set; }
        public string Table { get; set; }
    }

    [Route("/Schema")]
    [Route("/Schema/{SchemaFilter}")]
    [DefaultView("Schema")]
    public class TableSchemaQuery : IReturn<IEnumerable<TableSchema>>
    {
        public string SchemaFilter { get; set; }

        public TableSchemaQuery(string schemaFilter)
        {
            SchemaFilter = schemaFilter;
        }
    }
}