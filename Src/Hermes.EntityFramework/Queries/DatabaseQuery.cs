using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework.Queries
{
    public class DatabaseQuery : ISqlQuery, ISqlCommand
    {
        private readonly DbContext context;
 
        public DatabaseQuery(IContextFactory contextFactory)
        {
            context = contextFactory.GetContext();

            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
        }

        public List<TDto> SqlQuery<TDto>(string sqlQuery, params object[] parameters)
        {
            return context.Database.SqlQuery<TDto>(sqlQuery, parameters).ToList();
        }

        public void SqlCommand(string sqlQuery, params object[] parameters)
        {
            context.Database.ExecuteSqlCommand(sqlQuery, parameters);
        }
    }
}