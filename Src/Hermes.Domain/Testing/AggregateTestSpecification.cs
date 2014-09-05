using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Hermes.Equality;
using Hermes.Reflection;

namespace Hermes.Domain.Testing
{
    public interface IGiven<TAggregate> : IThen
        where TAggregate : class, IAggregate
    {
        IGiven<TAggregate> And(IAggregateEvent @event);
        IThen When(Expression<Action<TAggregate>> expression);
    }

    public interface IThen
    {
        void Then<TException>() where TException : Exception;
        void Then<TException>(string expectedExceptionName) where TException : DomainRuleException;
        void Then(params IAggregateEvent[] domainEvents);
    }

    public abstract class AggregateTestSpecification<TAggregate, TIdentity> :
        IGiven<TAggregate>, IThen
        where TAggregate : class, IAggregate
        where TIdentity : IIdentity
    {
        protected TAggregate Aggregate { get; private set; }
        private Exception thrownException;
        private CompareObjects comparer;
        protected abstract TIdentity DefaultIdentity { get; }

        protected AggregateTestSpecification()
        {
            Aggregate = null;
            BuildComparer();
        }

        private void BuildComparer()
        {
            comparer = new CompareObjects();
            comparer.AttributesToIgnore.Add(typeof(AggregateIdAttribute));
            comparer.ElementsToIgnore.Add("Version");
            comparer.MaxDifferences = 20;
        }

        protected IGiven<TAggregate> Given(IMemento memento)
        {
            try
            {
                Aggregate = ObjectFactory.CreateInstance<TAggregate>(DefaultIdentity);
                Aggregate.RestoreSnapshot(memento);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            return this;
        }

        protected IGiven<TAggregate> Given(Expression<Func<TAggregate>> expression)
        {
            try
            {
                Console.WriteLine("When : {0}", GetFormattedFunctionName(expression));
                Aggregate = expression.Compile()();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            return this;
        }

        IGiven<TAggregate> IGiven<TAggregate>.And(IAggregateEvent @event)
        {
            Console.WriteLine("And  : {0}", @event);
            Aggregate.ApplyEvent(@event);
            return this;
        }

        IThen IGiven<TAggregate>.When(Expression<Action<TAggregate>> expression)
        {
            try
            {
                Console.WriteLine("When : {0}", GetFormattedActionName(expression));
                Action<TAggregate> action = expression.Compile();
                action(Aggregate);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            return this;
        }

        private string GetFormattedFunctionName(Expression<Func<TAggregate>> function)
        {
            string name = function.ToString().Replace("() => ", "");
            int firstBracket = name.IndexOf('(');

            name = name.Remove(firstBracket);
            return InsertSpaces(name);
        }

        private static string InsertSpaces(string name)
        {
            return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        private string GetFormattedActionName(Expression<Action<TAggregate>> action)
        {
            string name = action.ToString();
            int firstDot = name.IndexOf('.');
            int firstBracket = name.IndexOf('(');

            name = name.Remove(firstBracket).Substring(firstDot + 1);
            return InsertSpaces(name);
        }

        public virtual void Then<TException>() where TException : Exception
        {
            if (thrownException == null)
                Fail("An exception of type {0} was expected but not thrown", typeof(TException).FullName);

            if (typeof(TException) != thrownException.GetType())
                Fail("The expected exception type does not match the thrown type. \n\tExpected Exception : {0} \n\tActual Thrown      : {1}",
                    typeof(TException).FullName, thrownException.GetType().FullName);

            Console.WriteLine("Then : {0} : {1}", typeof(TException).Name, thrownException.Message);
        }

        public virtual void Then<TException>(string expectedExceptionName) where TException : DomainRuleException
        {
            Then<TException>();

            if (!((DomainRuleException)thrownException).Name.Equals(expectedExceptionName, StringComparison.Ordinal))
                Fail("The expected exception name does not match the expected name . \n\tExpected Name : {0} \n\tActual Name   : {1}", expectedExceptionName,
                    ((DomainRuleException)thrownException).Name);

            Console.WriteLine("Then : {0} : {1} : {2}", typeof(TException).Name, expectedExceptionName, thrownException.Message);
        }

        public virtual void Then(params IAggregateEvent[] domainEvents)
        {
            if (thrownException != null)
                throw thrownException;

            IAggregateEvent[] raisedEvents = Aggregate.GetUncommittedEvents().ToArray();

            if (raisedEvents.Count() != domainEvents.Count())
                Fail("The aggregate {0} contained {1} events : Expected number of events is {2}",
                    Aggregate.GetType().Name, raisedEvents.Count(), domainEvents.Count());

            AssertEquality(raisedEvents, domainEvents);

            bool insertSpaces = false;

            foreach (var raisedEvent in raisedEvents)
            {
                if (insertSpaces)
                {
                    Console.WriteLine("       {0}", raisedEvent);
                }
                else
                {
                    insertSpaces = true;
                    Console.WriteLine("Then : {0}", raisedEvent);
                }
            }
        }

        private void AssertEquality(IAggregateEvent[] expected, IAggregateEvent[] actual)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                if (comparer.Compare(expected[index], actual[index]))
                {
                    continue;
                }

                Fail("Expected event did not match the actual event.\n{0}", comparer.DifferencesString);
            }
        }

        private void Fail(string message, params object[] parameters)
        {
            if (parameters == null)
            {
                Fail(message);
            }
            else
            {
                Fail(String.Format(message, parameters));
            }
        }

        private void Fail(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new AggregateSpecificationException();

            throw new AggregateSpecificationException(message);
        }
    }
}
