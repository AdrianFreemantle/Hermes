using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Hermes.Serialization;

namespace Hermes.Compression
{
    [DataContract]
    public struct CompressedObject : IEquatable<CompressedObject>
    {
        // ReSharper disable InconsistentNaming
        private static readonly CompressedObject empty = new CompressedObject();
        // ReSharper restore InconsistentNaming

        [DataMember(Name = "content")]
        private readonly byte[] content;

        [DataMember(Name = "contentType")]
        private readonly string contentType;

        public string ContentType
        {
            get { return contentType; }
        }

        public static CompressedObject Empty
        {
            get { return empty; }
        }

        public byte[] Content
        {
            get { return content; }
        }

        public CompressedObject(byte[] content, string contentType)
        {
            Mandate.ParameterNotNullOrEmpty(content, "value");
            Mandate.ParameterNotNullOrEmpty(contentType, "contentType");

            this.content = content;
            this.contentType = contentType;
        }

        public T ToObject<T>(ISerializeObjects serializer)
        {
            return serializer.DeserializeObject<T>(ToString());
        }

        public static CompressedObject Compress(object content, ISerializeObjects serializer)
        {
            string serialized = serializer.SerializeObject(content);
            byte[] bytes = CompressContent(Encoding.UTF8.GetBytes(serialized));

            return new CompressedObject(bytes, serializer.GetContentType());
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Decompress());
        }

        private static byte[] CompressContent(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return GzipCompressor.Compress(stream);
            }
        }

        private byte[] Decompress()
        {
            using (var decompressStream = GzipCompressor.Decompress(Content))
            using (var contentStream = new MemoryStream())
            {
                decompressStream.CopyTo(contentStream);
                return contentStream.ToArray();
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;

                foreach (var byteValue in Content)
                {
                    hash = (byteValue.GetHashCode() * 397) ^ hash;
                }

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is CompressedObject && Equals((CompressedObject)obj);
        }

        public bool Equals(CompressedObject other)
        {
            if (other.Content == null || other.Content.Length != Content.Length)
            {
                return false;
            }

            for (int i = Content.Length; i < Content.Length; i++)
            {
                if (Content[i] != other.Content[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(CompressedObject left, CompressedObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CompressedObject left, CompressedObject right)
        {
            return !Equals(left, right);
        }

        public static implicit operator byte[](CompressedObject content)
        {
            return content.Content;
        }
    }
}