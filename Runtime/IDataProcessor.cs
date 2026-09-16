namespace LB.LocalDataManager
{
    /// <summary>
    /// A text-to-text stage between serializing and writing: encryption, compression,
    /// obfuscation, a checksum. Applied after <see cref="IDataSerializer.Serialize{T}"/> on
    /// the way out and before <see cref="IDataSerializer.Deserialize{T}"/> on the way back,
    /// so it composes with any serializer.
    /// </summary>
    /// <remarks>
    /// <see cref="Decode"/> is expected to throw when the data cannot be restored - a wrong
    /// key or a tampered file, say. <see cref="LocalDataLoader"/> turns that into a failed
    /// load rather than letting it escape.
    /// </remarks>
    public interface IDataProcessor
    {
        /// <summary>Transforms serialized text into what gets written to disk.</summary>
        string Encode(string data);

        /// <summary>
        /// Reverses <see cref="Encode"/>. Throws when the input is not something this
        /// processor produced.
        /// </summary>
        string Decode(string data);
    }
}
