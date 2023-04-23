namespace DUJAL.Systems.Dialogue 
{
    using System;
    using UnityEngine;
    [Serializable]
    public class ChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
