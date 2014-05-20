using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Hermes.Persistence;
using Hermes.Serialization;

namespace Hermes.EntityFramework.KeyValueStore
{
    public class KeyValueStorePersister : IKeyValueStore
    {
        private readonly Encoding encoding = Encoding.UTF8;

        private readonly ISerializeObjects serializer;
        private readonly IDbSet<KeyValueEntity> repository;

        public KeyValueStorePersister(IRepositoryFactory repositoryFactory, ISerializeObjects serializer)
        {
            this.serializer = serializer;
            repository = repositoryFactory.GetRepository<KeyValueEntity>();
        }

        string ToHash(dynamic key)
        {
            byte[] hashBytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key.ToString()));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public void Add(dynamic key, object value)
        {
            Mandate.ParameterNotNull(key, "Please provide a non-null key");
            Mandate.ParameterNotNull(value, "Please provide a non null value to store.");

            string hash = ToHash(key);

            var entity = new KeyValueEntity
            {
                Hash = hash,
                Key = key.ToString(),
                ValueType = value.GetType().AssemblyQualifiedName,
                Value = Serialize(value)
            };

            repository.Add(entity);
        }

        public void Update(dynamic key, object value)
        {
            Mandate.ParameterNotNull(key, "Please provide a non-null key");
            Mandate.ParameterNotNull(value, "Please provide a non-null value to update.");

            var entity = GetEntity(key);
            entity.Value = Serialize(value);
            entity.ValueType = value.GetType().AssemblyQualifiedName;
        }        

        public object Get(dynamic key)
        {
            Mandate.ParameterNotNull(key, "Please provide a non-null key");

            var entity = GetEntity(key);

            if (entity == null)
            {
                return null;
            }

            return Deserialize(entity.Value, Type.GetType(entity.ValueType));
        }

        public void Remove(dynamic key)
        {
            Mandate.ParameterNotNull(key, "Please provide a non-null key");

            var entity = GetEntity(key);

            if (entity == null)
            {
                return;
            }

            repository.Remove(entity);
        }

        private KeyValueEntity GetEntity(dynamic key)
        {
            try
            {
                string hash = ToHash(key);
                return repository.FirstOrDefault(valueEntity => valueEntity.Hash == hash) ?? repository.First(valueEntity => valueEntity.Hash == hash);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(String.Format("No value was found in the key-value-store that matches key {0}", key.ToString()));
            }
        }

        public object Deserialize(byte[] body, Type objectType)
        {
            if (body == null || body.Length == 0)
            {
                return new object();
            }

            var serialized = encoding.GetString(body);
            return serializer.DeserializeObject(serialized, objectType);
        }

        public byte[] Serialize(object value)
        {
            var serialized = serializer.SerializeObject(value);
            return encoding.GetBytes(serialized);
        }
    }
}