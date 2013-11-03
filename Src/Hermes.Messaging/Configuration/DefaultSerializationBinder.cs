using System;
using System.Runtime.Serialization;

namespace Hermes.Messaging.Configuration
{
    public class DefaultSerializationBinder : SerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.AssemblyQualifiedName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }
}