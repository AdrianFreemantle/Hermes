using System;
using System.Runtime.Serialization;
using Hermes.Configuration;

namespace Hermes.Messaging
{
    ///<summary>
    /// Abstraction for an address on the NServiceBus network.
    ///</summary>
    [DataContract]
    public class Address 
    {
        private const int queueIndex = 0;
        private const int machineIndex = 1;
        private readonly string queueLowerCased;
        private readonly string machineLowerCased;
        private static readonly Address undefined;
        private static Address local;
        private static bool ignoreMachineName;

        [DataMember(Order = 1, EmitDefaultValue = false, IsRequired = false)]
        private readonly string queue;

        [DataMember(Order = 2, EmitDefaultValue = false, IsRequired = false)]
        private readonly string machine;

        /// <summary>
        /// The name of the queue not including the name of the machine or location depending on the address mode.
        /// </summary>
        public string Queue { get { return queue; } }

        /// <summary>
        /// The  name of the machine or the (normal) name of the location depending on the address mode.
        /// </summary>
        public string Machine { get { return machine; } }

        /// <summary>
        /// Get the address of this endpoint.
        /// </summary>
        public static Address Local { get { return local; } }

        /// <summary>
        /// Undefined address.
        /// </summary>
        public static  Address Undefined { get { return undefined; } }

        static Address()
        {
            undefined = new Address("__UNDEFINED__", String.Empty);
            local = undefined;
        }

        /// <summary>
        /// Instantiate a new Address for a known queue on a given machine.
        /// </summary>
        ///<param name="queueName">The queue name.</param>
        ///<param name="machineName">The machine name.</param>
        public Address(string queueName, string machineName)
        {
            Mandate.ParameterNotNullOrEmpty(queueName, "queueName", "Invalid queue name specified");

            queue = queueName;
            queueLowerCased = queue.ToLower();

            machine = machineName ?? RuntimeEnvironment.GetMachineName();
            machine = Machine;
            machineLowerCased = machine.ToLower();
        }

        /// <summary>
        /// Instantiate a new Address for a known queue on a given machine.
        /// </summary>
        ///<param name="queueName">The queue name.</param>
        public Address(string queueName)
            : this(queueName, RuntimeEnvironment.GetMachineName())
        {
        }

        /// <summary>
        /// Parses a string and returns an Address.
        /// </summary>
        /// <param name="destination">The full address to parse.</param>
        /// <returns>A new instance of <see cref="Address"/>.</returns>
        public static Address Parse(string destination)
        {
            Mandate.ParameterNotNullOrEmpty(destination, "destination", "Invalid destination address specified");
            
            var address = destination.Split('@');
            
            Mandate.ParameterNotNullOrEmpty(address[queueIndex], "destination", "Invalid destination address specified");
        
            switch (address.Length)
            {
                case 1:
                    return new Address(address[queueIndex], RuntimeEnvironment.GetMachineName());
                case 2:
                    Mandate.ParameterNotNullOrEmpty(address[machineIndex], "destination", "Invalid machine name specified in destination address");
                    return new Address(address[queueIndex], address[machineIndex]);
                default:
                    throw new ArgumentException("Invalid destination address specified", "destination"); 
            }
        }

        /// <summary>
        /// Sets the address of this endpoint.
        /// </summary>
        /// <param name="queue">The queue name.</param>
        public static void InitializeLocalAddress(string queue)
        {
            local = Parse(queue);
        }

        /// <summary>
        /// Instructed the address to not consider the machine name
        /// </summary>
        public static void IgnoreMachineName()
        {
            ignoreMachineName = true;
        }

        /// <summary>
        /// Creates a new Address whose Queue is derived from the Queue of the existing Address
        /// together with the provided qualifier. For example: queue.qualifier@machine
        /// </summary>
        /// <param name="qualifier"></param>
        /// <returns></returns>
        public Address SubScope(string qualifier)
        {
            return new Address(Queue + "." + qualifier, Machine);
        }

        /// <summary>
        /// Provides a hash code of the Address.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return GetQueueHashCode() ^ GetMachineHashCode();
            }
        }

        private int GetMachineHashCode()
        {
            return (machineLowerCased != null ? machineLowerCased.ToLower().GetHashCode() : 0);
        }

        private int GetQueueHashCode()
        {
            return ((queueLowerCased != null ? queueLowerCased.GetHashCode() : 0) * 397);
        }

        /// <summary>
        /// Returns a string representation of the address.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ignoreMachineName)
                return Queue;

            return Queue + "@" + Machine;
        }

        /// <summary>
        /// Overloading for the == for the class Address
        /// </summary>
        /// <param name="left">Left hand side of == operator</param>
        /// <param name="right">Right hand side of == operator</param>
        /// <returns>true if the LHS is equal to RHS</returns>
        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloading for the != for the class Address
        /// </summary>
        /// <param name="left">Left hand side of != operator</param>
        /// <param name="right">Right hand side of != operator</param>
        /// <returns>true if the LHS is not equal to RHS</returns>
        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Address)) return false;
            return Equals((Address)obj);
        }

        /// <summary>
        /// Check this is equal to other Address
        /// </summary>
        /// <param name="other">reference addressed to be checked with this</param>
        /// <returns>true if this is equal to other</returns>
        private bool Equals(Address other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (!ignoreMachineName && !other.machine.Equals(machine))
                return false;

            return other.queue.Equals(queue);
        }
    }
}
