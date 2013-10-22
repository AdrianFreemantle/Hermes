namespace Hermes.Persistence
{
    public interface IKeyValueStore
    {
        void Add(dynamic key, object value);
        void Update(dynamic key, object value);
        object Get(dynamic key);
    }
}
