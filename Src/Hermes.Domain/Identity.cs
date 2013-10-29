using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Hermes.Domain
{
    [Serializable]
    [DataContract]
    //[DebuggerStepThrough]
    public abstract class Identity<T> : IIdentity, IEquatable<Identity<T>>, IEquatable<T>
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly Type[] SupportTypes = {typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(Guid), typeof(string)};
        // ReSharper restore StaticFieldInGenericType

        [DataMember]
        private readonly T id;

        protected T Id
        {
            get { return id; }
        }

        protected Identity()
        {
        }

        protected Identity(T id)
        {
            VerifyIdentityType(id);
            this.id = id;
        }

        public virtual string GetTag()
        {
            var typeName = GetType().Name;
            
            return typeName.EndsWith("Id") 
                ? typeName.Substring(0, typeName.Length - 2) 
                : typeName;
        }                        

        public dynamic GetId()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var identity = obj as Identity<T>;

            if (identity != null)
            {
                return Equals(identity);
            }

            if (obj is T)
            {
                return Equals((T)obj);
            }

            return false;
        }

        public bool Equals(T other)
        {
            return Id.Equals(other);
        }

        public bool Equals(Identity<T> other)
        {
            if (other != null)
            {
                return other.Id.Equals(Id) && other.GetTag() == GetTag();
            }

            return false;
        }        

        public override string ToString()
        {
            return string.Format("{0}-{1}", GetTag(), Id);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode());
        }

        void VerifyIdentityType(T identity)
        {
            if (ReferenceEquals(identity, null))
            {
                throw new ArgumentException("You must provide a non null value as an identity");
            }

            var type = identity.GetType();

            if (SupportTypes.Any(t => t == type))
            {
                return;
            }

            throw new InvalidOperationException("Abstract identity inheritors must provide stable hash. It is not supported for:  " + type);
        }

        public virtual bool IsEmpty()
        {
            return Id.Equals(default(T));
        }

        public static bool operator ==(Identity<T> left, Identity<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Identity<T> left, Identity<T> right)
        {
            return !Equals(left, right);
        }
    }
}