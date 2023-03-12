using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue
{
    public class DialogueScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<ChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public AudioClip VoiceLine { get; set; }
        [field: SerializeField] public Sprite SpeakerSprite { get; set; }

        public void Initialize(string dialogueName, string text, List<ChoiceData> choices, DialogueType dialogueType, bool isSatartingDialogue) 
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isSatartingDialogue;
        }
    }
}