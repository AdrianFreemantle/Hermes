using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Hermes.EntityFramework
{
    public class Repository<TEntity> : IDbSet<TEntity> where TEntity : class
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly InMemoryRepository InMemoryRepository = new InMemoryRepository();

        public IEnumerator<TEntity> GetEnumerator()
        {
            return InMemoryRepository.GetQueryable<TEntity>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; private set; }
        public Type ElementType { get; private set; }
        public IQueryProvider Provider { get; private set; }
        public TEntity Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public TEntity Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Remove(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Attach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Create()
        {
            throw new NotImplementedException();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<TEntity> Local { get; private set; }
    }
}