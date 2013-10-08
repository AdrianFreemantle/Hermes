using System;
using System.Security.Cryptography;
using System.Text;

using Hermes.EntityFramework;
using Hermes.Serialization;

namespace Hermes.Storage.EntityFramework.KeyValueStore
{
    public class KeyValueStorePersister : IKeyValueStore
    {
        private readonly Encoding encoding = Encoding.UTF8;

        private readonly IUnitOfWork unitOfWork;
        private readonly ISerializeMessages serializer;

        public KeyValueStorePersister(IUnitOfWork unitOfWork, ISerializeMessages serializer)
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
            string id = ToHash(key);

            var repository = unitOfWork.GetRepository<KeyValueEntity>();

            var entity = new KeyValueEntity
            {
                Id = id,
                Key = key.ToString(),
                Value = serializer.Serialize(new[] {value})
            };

            repository.Add(entity);
        }

        public void Update(dynamic key, object value)
        {
            string id = ToHash(key);

            var repository = unitOfWork.GetRepository<KeyValueEntity>();
            KeyValueEntity entity = repository.Get(id);
            entity.Value = serializer.Serialize(new[] {value});
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

            return serializer.Deserialize(entity.Value)[0];
        }
    }
}