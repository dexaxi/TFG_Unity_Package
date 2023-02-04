using System.Collections.Generic;

namespace DUJAL.Systems.Utils
{
    using UnityEngine;

    /// <summary>
    //  SerializableDictionary class, as Unity does not support serializable dictionaries.
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();

            Debug.Assert(keys.Count == values.Count, "SerializableDictionary: Tried to Deserialize a SerializableDictionary, but the amount of keys: "
                + keys.Count + " and the amount of values: " + values.Count + " do not correspond.");

            for (int i = 0; i < keys.Count; i++)
            {
                this.Add(keys[i], values[i]);
            }
        }

        /// <summary>
        //  Function to copy SerializableDictionaries by value when saving.
        /// </summary>
        public void CopySerializableDictionary(SerializableDictionary<TKey, TValue> reference)
        {
            reference.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                reference.Add(pair.Key, pair.Value);
            }
        }
    }
}
