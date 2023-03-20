using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DUJAL.Systems.Utils;
namespace DUJAL.Systems.Dialogue 
{
    public class DialogueContainerScriptableObject : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<GroupScriptableObject, List<DialogueScriptableObject>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DialogueScriptableObject> UngroupedDialogues { get; set; }

        public void Initialize(string filename) 
        {
            FileName = filename;
            DialogueGroups = new SerializableDictionary<GroupScriptableObject, List<DialogueScriptableObject>>();
            UngroupedDialogues = new List<DialogueScriptableObject>();
        }
    }
}
