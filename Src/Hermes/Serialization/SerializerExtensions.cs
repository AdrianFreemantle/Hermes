using System;
using System.Text;
using Hermes.Compression;
using Hermes.Encryption;

namespace Hermes.Serialization
{
    public static class SerializerExtensions
    {
        public static byte[] ToByteArray(this ISerializeObjects serializer, object value)
        {
            var serialized = serializer.SerializeObject(value);
            return Encoding.UTF8.GetBytes(serialized);
        }

        public static T FromByteArray<T>(this ISerializeObjects serializer, byte[] value)
        {
            var serialized = Encoding.UTF8.GetString(value);
            return serializer.DeserializeObject<T>(serialized);
        }

        public static CompressedObject Compress(this ISerializeObjects serializer, object value)
        {
            return CompressedObject.Compress(value, serializer, CompressionOption.Gzip);
        }

        public static T Decompress<T>(this ISerializeObjects serializer, CompressedObject compressedObject)
        {
            if (compressedObject.ContentType != serializer.GetContentType())
            {
                var message = String.Format("The serializer format of {0} is not compatible with the compressed object's format of {1}.", compressedObject.ContentType, serializer.GetContentType());
                throw new FormatException(message);
            }

            return serializer.DeserializeObject<T>(compressedObject.ToString());
        }

        public static byte[] Encrypt(this ISerializeObjects serializer, object objectToBeEncrypted, string password, string salt)
        {
            byte[] bytesToBeEncrypted = serializer.ToByteArray(objectToBeEncrypted);
            byte[] passwordBytes = serializer.ToByteArray(password);
            byte[] saltBytes = serializer.ToByteArray(salt);

            return AesEncryption.Encrypt(bytesToBeEncrypted, passwordBytes, saltBytes);
        }

        public static T Decrypt<T>(this ISerializeObjects serializer, byte[] bytesToBeDecrypted, string password, string salt)
        {
            byte[] passwordBytes = serializer.ToByteArray(password);
            byte[] saltBytes = serializer.ToByteArray(salt);
            byte[] decryptedBytes = AesEncryption.Decrypt(bytesToBeDecrypted, passwordBytes, saltBytes);

            return serializer.FromByteArray<T>(decryptedBytes);
        }
    }
}