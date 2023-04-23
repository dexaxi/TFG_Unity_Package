namespace DUJAL.Systems.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine;

    public class DialogueScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<ChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public AudioClip VoiceLine { get; set; }
        [field: SerializeField] public Sprite SpeakerSprite { get; set; }
        [field: SerializeField] public Color DialogueColor { get; set; }

        public void Initialize(string dialogueName, string text, List<ChoiceData> choices, DialogueType dialogueType, bool isStartingDialogue, AudioClip clip, Sprite sprite, Color dialogueColor) 
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            VoiceLine = clip;
            SpeakerSprite = sprite;
            DialogueColor = dialogueColor;
        }
        public static DialogueScriptableObject CopyInto(DialogueScriptableObject origin, DialogueScriptableObject copy) 
        {
            copy.DialogueName = origin.DialogueName; 
            copy.Text = origin.Text;
            copy.Choices = new List<ChoiceData>();
            foreach (ChoiceData c in origin.Choices) 
            {
                copy.Choices.Add(c);
            }
            copy.DialogueType = origin.DialogueType;
            copy.IsStartingDialogue = origin.IsStartingDialogue;
            copy.VoiceLine = origin.VoiceLine;
            copy.SpeakerSprite = origin.SpeakerSprite;
            copy.DialogueColor = origin.DialogueColor;
            return copy;
        }
    }
}