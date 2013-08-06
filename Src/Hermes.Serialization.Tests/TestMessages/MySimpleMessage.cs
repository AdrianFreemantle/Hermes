using System;

namespace Hermes.Serialization.Tests.TestMessages
{
    public class MySimpleMessage 
    {
        public Guid Id { get; set; }
        public DateTime Sent { get; set; }
        public string Text { get; set; }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MySimpleMessage);
        }

        public virtual bool Equals(MySimpleMessage other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.Id == Id
                       && other.Sent == Sent
                       && other.Text == Text;
            }

            return false;
        }

        public static bool operator ==(MySimpleMessage left, MySimpleMessage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MySimpleMessage left, MySimpleMessage right)
        {
            return !Equals(left, right);
        }
    }
}