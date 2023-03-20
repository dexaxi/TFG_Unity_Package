using UnityEngine;

namespace DUJAL.Systems.Dialogue
{ 
    public class ChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueScriptableObject NextDialogue { get;  set; }
    }
}
