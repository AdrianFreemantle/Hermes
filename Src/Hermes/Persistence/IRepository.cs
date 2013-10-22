﻿using System;

namespace Hermes.Persistence
{
    public interface IRepository<TEntity>
    {
        TEntity Get(int id);
        TEntity Get(long id);
        TEntity Get(uint id);
        TEntity Get(ulong id);
        TEntity Get(Guid id);
        TEntity Get(string id);
        void Add(TEntity newEntity);
        void Remove(TEntity entity);
    }
}
