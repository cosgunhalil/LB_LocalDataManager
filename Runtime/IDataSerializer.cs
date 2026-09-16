namespace LB.LocalDataManager
{
    /// <summary>
    /// Turns objects into text and back. Implement this to save with something other than
    /// <see cref="UnityEngine.JsonUtility"/> - Newtonsoft.Json, for instance, which lifts the
    /// limits the default serializer imposes on what a saved type may contain.
    /// </summary>
    /// <remarks>
    /// Implementations are used from <see cref="LocalDataSaver"/> and
    /// <see cref="LocalDataLoader"/>, which report a throwing serializer as a failed save or
    /// load. They must be safe to call from more than one instance at a time.
    /// </remarks>
    public interface IDataSerializer
    {
        /// <summary>Serializes <paramref name="value"/> to the text that gets written.</summary>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes <paramref name="data"/>, which is whatever
        /// <see cref="Serialize{T}"/> produced. May throw; callers handle it.
        /// </summary>
        T Deserialize<T>(string data);
    }
}
