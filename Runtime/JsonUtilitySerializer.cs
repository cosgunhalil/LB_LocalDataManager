using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Default <see cref="IDataSerializer"/>, backed by <see cref="JsonUtility"/>.
    /// <para>
    /// It inherits Unity's serialization rules, which are the package's default contract: a
    /// <c>[Serializable]</c> class or struct with public (or <c>[SerializeField]</c>) fields,
    /// no properties, no <c>Dictionary</c>, no polymorphism, and no collection or primitive
    /// as the top-level type. Swap in another <see cref="IDataSerializer"/> to escape them.
    /// </para>
    /// </summary>
    public sealed class JsonUtilitySerializer : IDataSerializer
    {
        private readonly bool _prettyPrint;

        /// <summary>Writes compact JSON.</summary>
        public JsonUtilitySerializer() : this(false)
        {
        }

        /// <param name="prettyPrint">
        /// Write indented JSON. Useful while developing, since the saved file is then
        /// readable; it costs file size.
        /// </param>
        public JsonUtilitySerializer(bool prettyPrint)
        {
            _prettyPrint = prettyPrint;
        }

        public string Serialize<T>(T value)
        {
            return JsonUtility.ToJson(value, _prettyPrint);
        }

        public T Deserialize<T>(string data)
        {
            return JsonUtility.FromJson<T>(data);
        }
    }
}
