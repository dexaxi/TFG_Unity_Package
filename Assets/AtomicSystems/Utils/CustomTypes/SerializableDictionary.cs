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
            keys = new List<TKey>(this.Count);
            values = new List<TValue>(this.Count);

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            if (keys != null && values != null && keys.Count == values.Count) 
            {
                this.Clear();
                for (int i = 0; i < keys.Count; i++)
                {
                    this[keys[i]] = values[i];
                }
                keys = null;
                values = null;
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
