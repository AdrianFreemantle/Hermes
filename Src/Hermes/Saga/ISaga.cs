namespace Hermes.Saga
{
    public interface ISaga<T> where T : class, IContainSagaData
    {
        T State { get; set; }
    }
}