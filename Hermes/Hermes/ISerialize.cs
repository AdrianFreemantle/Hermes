using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes
{
    /// <summary>
    /// Interface for serializers that can read/write an object graph to a stream.
    /// </summary>
    public interface ISerialize
    {
        /// <summary>
        /// Serializes an object graph to a text reader.
        /// </summary>
        void Serialize<T>(Stream output, T graph);

        /// <summary>
        /// Deserializes an object graph from the specified text reader.
        /// </summary>
        T Deserialize<T>(Stream input);
    }
}
