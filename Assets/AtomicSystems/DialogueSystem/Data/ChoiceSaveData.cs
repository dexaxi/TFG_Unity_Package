using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue 
{
    [Serializable]
    public class ChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
