using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hermes.EntityFramework
{
    public sealed class InMemoryRepository 
    {
        readonly Dictionary<Type, object> repository = new Dictionary<Type, object>();

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            return GetCollection<TEntity>().AsQueryable();
        }

        public TEntity Get<TEntity>(object id) where TEntity : class
        {
            PropertyInfo keyProperty = GetKeyProperty<TEntity>();
            Expression<Func<TEntity, bool>> lambda = BuildLambdaExpressionForKey<TEntity>(id, keyProperty);
            IQueryable<TEntity> collectionQuery = GetCollection<TEntity>().AsQueryable();

            return collectionQuery.Single(lambda);
        }

        private static Expression<Func<TEntity, bool>> BuildLambdaExpressionForKey<TEntity>(object id, PropertyInfo keyProperty) where TEntity : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression property = Expression.Property(parameter, keyProperty.Name);
            Expression target = Expression.Constant(id);
            Expression equalsMethod = Expression.Equal(property, target);
            return Expression.Lambda<Func<TEntity, bool>>(equalsMethod, parameter);
        }

        private static PropertyInfo GetKeyProperty<TEntity>() where TEntity : class
        {
            return typeof(TEntity)
                .GetProperties()
                .Single(propertyInfo => Attribute.IsDefined(propertyInfo, typeof(KeyAttribute)));
        }

        public TEntity Add<TEntity>(TEntity item) where TEntity : class
        {
            HashSet<TEntity> collection = GetCollection<TEntity>();
            collection.Add(item);
            return item;
        }

        public TEntity Remove<TEntity>(TEntity item) where TEntity : class
        {
            HashSet<TEntity> collection = GetCollection<TEntity>();

            collection.Remove(item);
            return item;
        }

        private HashSet<TEntity> GetCollection<TEntity>() where TEntity : class
        {
            if (!repository.ContainsKey(typeof(TEntity)))
            {
                var collection = new HashSet<TEntity>();
                repository.Add(typeof(TEntity), collection);
                return collection;
            }

            return repository[typeof(TEntity)] as HashSet<TEntity>;
        }
    }
}