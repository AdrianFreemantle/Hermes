using System;
using System.Security.Cryptography;
using System.Text;

using Hermes.Persistence;
using Hermes.Serialization;

namespace Hermes.EntityFramework.KeyValueStore
{
    public class KeyValueStorePersister : IKeyValueStore
    {
        private readonly Encoding encoding = Encoding.UTF8;

        private readonly IEntityUnitOfWork unitOfWork;
        private readonly ISerializeObjects serializer;

        public KeyValueStorePersister(IEntityUnitOfWork unitOfWork, ISerializeObjects serializer)
        {
            this.unitOfWork = unitOfWork;
            this.serializer = serializer;
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

            string id = ToHash(key);

            var repository = unitOfWork.GetRepository<KeyValueEntity>();

            var entity = new KeyValueEntity
            {
                Id = id,
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

            string id = ToHash(key);

            var repository = unitOfWork.GetRepository<KeyValueEntity>();
            KeyValueEntity entity = repository.Get(id);
            entity.Value = Serialize(value);
            entity.ValueType = value.GetType().AssemblyQualifiedName;
        }

        public object Get(dynamic key)
        {
            string id = ToHash(key);

            var repository = unitOfWork.GetRepository<KeyValueEntity>();
            var entity = repository.Get(id);

            if (entity == null)
            {
                return null;
            }

            return Deserialize(entity.Value, Type.GetType(entity.ValueType));
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