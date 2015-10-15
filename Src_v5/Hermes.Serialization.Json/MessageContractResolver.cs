using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Hermes.Reflection;

using Newtonsoft.Json.Serialization;

namespace Hermes.Serialization.Json
{
    public class MessageContractResolver : DefaultContractResolver
    {
        private readonly TypeMapper typeMapper;

        public MessageContractResolver(TypeMapper typeMapper)
            : base(true)
        {
            this.typeMapper = typeMapper;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var mappedTypeFor = typeMapper.GetMappedTypeFor(objectType);

            if (mappedTypeFor == null)
                return base.CreateObjectContract(objectType);

            var jsonContract = base.CreateObjectContract(mappedTypeFor);
            jsonContract.DefaultCreator = () => typeMapper.CreateInstance(mappedTypeFor);

            return jsonContract;
        }
    }
}
