
namespace DUJAL.Systems.Dialogue
{
    using System;
    using UnityEngine;
    [Serializable]
    public class ChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueScriptableObject NextDialogue { get;  set; } 
    }
}
