using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventStore;

using Hermes.Core;
using Hermes.Ioc;
using Hermes.Messaging;

namespace MyDomain.Infrastructure
{
    public interface IEventStoreRepository : IDisposable
    {
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
        TAggregate GetById<TAggregate>(Guid id, int versionToLoad) where TAggregate : class, IAggregate;
        void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
    }

    public class EventStoreRepository : IEventStoreRepository
    {
        private const string AggregateTypeHeader = "AggregateType";
        private readonly IDictionary<Guid, Snapshot> snapshots = new Dictionary<Guid, Snapshot>();
        private readonly IDictionary<Guid, IEventStream> streams = new Dictionary<Guid, IEventStream>();
        private readonly IStoreEvents eventStore;

        public EventStoreRepository(IStoreEvents eventStore)
        {
            this.eventStore = eventStore;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            lock (streams)
            {
                foreach (var stream in streams)
                {
                    stream.Value.Dispose();
                }

                snapshots.Clear();
                streams.Clear();
            }
        }

        public virtual TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(id, int.MaxValue);
        }

        public virtual TAggregate GetById<TAggregate>(Guid id, int versionToLoad) where TAggregate : class, IAggregate
        {
            var snapshot = GetSnapshot(id, versionToLoad);
            var stream = OpenStream(id, versionToLoad, snapshot);
            var aggregate = GetAggregate<TAggregate>(snapshot, stream);

            ApplyEventsToAggregate(versionToLoad, stream, aggregate);

            return aggregate as TAggregate;
        }

        private static void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
        {
            if (versionToLoad == 0 || aggregate.GetVersion() < versionToLoad)
            {
                aggregate.LoadFromHistory(stream.CommittedEvents.Select(x => x.Body as DomainEvent));
            }
        }

        private IAggregate GetAggregate<TAggregate>(Snapshot snapshot, IEventStream stream) where TAggregate : class, IAggregate
        {
            IMemento memento = snapshot == null ? null : snapshot.Payload as IMemento;

            var aggregate = ActivatorHelper.CreateInstanceUsingNonPublicConstructor<TAggregate>(stream.StreamId);
            aggregate.RestoreSnapshot(memento);
            return aggregate;
        }

        private Snapshot GetSnapshot(Guid id, int version)
        {
            Snapshot snapshot;

            if (!snapshots.TryGetValue(id, out snapshot))
            {
                snapshots[id] = snapshot = eventStore.Advanced.GetSnapshot(id, version);
            }

            return snapshot;
        }

        private IEventStream OpenStream(Guid id, int version, Snapshot snapshot)
        {
            IEventStream stream;

            if (streams.TryGetValue(id, out stream))
            {
                return stream;
            }

            stream = snapshot == null
                ? eventStore.OpenStream(id, 0, version)
                : eventStore.OpenStream(snapshot, version);

            return streams[id] = stream;
        }

        public virtual void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = PrepareHeaders(aggregate, updateHeaders);
           
            while (true)
            {
                var stream = PrepareStream(aggregate, headers);

                try
                {
                    stream.CommitChanges(commitId);
                    aggregate.ClearUncommittedEvents();
                    return;
                }
                catch (ConcurrencyException)
                {
                    streams.Remove((Guid)aggregate.Identity.GetId());
                    throw;
                }
                //catch (Exception)
                //{
                //    stream.ClearChanges();
                //    throw;
                //}
                //catch (DuplicateCommitException)
                //{
                //    stream.ClearChanges();
                //    throw;
                //}

            }
        }

        private IEventStream PrepareStream(IAggregate aggregate, Dictionary<string, object> headers)
        {
            IEventStream stream;

            if (!streams.TryGetValue((Guid)aggregate.Identity.GetId(), out stream))
            {
                streams[(Guid)aggregate.Identity.GetId()] = stream = eventStore.CreateStream((Guid)aggregate.Identity.GetId());
            }

            foreach (var item in headers)
                stream.UncommittedHeaders[item.Key] = item.Value;

            aggregate.GetUncommittedEvents()
                .Select(x => new EventMessage { Body = x })
                .ToList()
                .ForEach(stream.Add);

            return stream;
        }

        private static Dictionary<string, object> PrepareHeaders(IAggregate aggregate, Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>();

            headers[AggregateTypeHeader] = aggregate.GetType().FullName;
            if (updateHeaders != null)
                updateHeaders(headers);

            return headers;
        }
    }

    public static class ActivatorHelper
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static T CreateInstance<T>(params object[] parameters) where T : class
        {
            if (parameters.Length == 0)
                return Activator.CreateInstance(typeof(T)) as T;

            return Activator.CreateInstance(typeof(T), parameters) as T;
        }

        public static T CreateInstanceUsingNonPublicConstructor<T>(params object[] parameters) where T : class
        {
            Type[] types = parameters.ToList().ConvertAll(input => input.GetType()).ToArray();

            var constructor = typeof(T).GetConstructor(Flags, null, types, null);

            return constructor.Invoke(parameters) as T;
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>(string assemblyName) where TBase : class
        {
            var assembly = Assembly.Load(assemblyName);
            var concreteSubTypes = GetConcreteSubTypes<TBase>(assembly);
            return CreateInstancesImplimentingBase<TBase>(concreteSubTypes);
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>() where TBase : class
        {
            var assembly = Assembly.GetAssembly(typeof(TBase));
            var concreteSubTypes = GetConcreteSubTypes<TBase>(assembly);
            return CreateInstancesImplimentingBase<TBase>(concreteSubTypes);
        }

        public static List<TBase> CreateInstancesImplimentingBase<TBase>(IEnumerable<Type> concreteTypes) where TBase : class
        {
            return concreteTypes.Select(type => Activator.CreateInstance(type) as TBase).ToList();
        }

        public static IEnumerable<Type> GetConcreteSubTypes<TBase>(Assembly sourceAssembly) where TBase : class
        {
            var assignableFromTypes = sourceAssembly.GetTypes().Where(type => type.IsAssignableFrom(typeof(TBase)));
            var implimentingInterfaceTypes = sourceAssembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(TBase)));

            return assignableFromTypes.Union(implimentingInterfaceTypes).Where(type => !type.IsAbstract && type.IsClass).ToList();
        }
    }
}
