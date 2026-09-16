using System;

namespace LB.LocalDataManager.Samples
{
    /// <summary>
    /// Shape a saved type has to have: [Serializable], public fields (not properties),
    /// no dictionaries, not a collection at the top level.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public int Id;
        public string Name;
    }
}
