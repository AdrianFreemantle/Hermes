using System;
using System.Runtime.Serialization;

namespace Hermes.Serialization.Tests.TestMessages
{
    [DataContract(Name = "MyDataContractMessage", Namespace = "Serialization.Tests")]
    public class MyDataContractMessage 
    {
        [DataMember(Order = 1)]
        public Guid Id { get; private set; }

        [DataMember(Order = 2)]
        public DateTime Sent { get; private set; }

        [DataMember(Order = 3)]
        public string Text { get; private set; }

        protected MyDataContractMessage() { }

        public MyDataContractMessage(Guid id, DateTime sent, string text)
        {
            Id = id;
            Sent = sent;
            Text = text;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MyDataContractMessage);
        }

        public virtual bool Equals(MyDataContractMessage other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.Id == Id
                       && other.Sent == Sent
                       && other.Text == Text;
            }

            return false;
        }

        public static bool operator ==(MyDataContractMessage left, MyDataContractMessage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MyDataContractMessage left, MyDataContractMessage right)
        {
            return !Equals(left, right);
        }
    }
}