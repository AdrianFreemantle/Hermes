using System;

namespace Hermes.Serialization
{
    public interface ISerializeObjects
    {
        T DeserializeObject<T>(string value);
        object DeserializeObject(string value, Type type);
        string SerializeObject(object value);
        string GetContentType();
    }
}