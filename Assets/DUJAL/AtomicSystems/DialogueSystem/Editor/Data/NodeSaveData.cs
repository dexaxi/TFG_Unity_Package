using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUJAL.Systems.Dialogue 
{
    [Serializable]
    public class NodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<ChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public Color BackgroundColor { get; set; }
        [field: SerializeField] public AudioClip VoiceLine { get; set; }
        [field: SerializeField] public Sprite SpeakerSprite { get; set; }
    }
}
