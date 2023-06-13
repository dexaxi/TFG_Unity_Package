namespace DUJAL.Systems.Dialogue 
{
    using System.Collections.Generic;
    using UnityEngine;
    using DUJAL.Systems.Utils;
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

        public List<string> GetDialogueGroupNames() 
        {
            List<string> returnableNames = new List<string>();
            foreach (GroupScriptableObject groupSO in DialogueGroups.Keys) 
            {
                returnableNames.Add(groupSO.GroupName);
            }
            return returnableNames;
        }

        public List<string> GetGroupedDialogueNames(GroupScriptableObject groupSO, bool startingDialogueFilter)
        {
            List<DialogueScriptableObject> dialoguesInGroup = DialogueGroups[groupSO];
            List<string> returnableNames = new List<string>();
            foreach (DialogueScriptableObject dialogueSO in dialoguesInGroup)
            {
                if (startingDialogueFilter && dialogueSO.IsStartingDialogue)
                {
                    returnableNames.Add(dialogueSO.DialogueName);
                }
                else if (!startingDialogueFilter) 
                {                
                    returnableNames.Add(dialogueSO.DialogueName);
                }
            }
            return returnableNames;
        }


        public List<string> GetUnGroupedDialogueNames(bool startingDialogueFilter)
        {
            List<string> returnableNames = new List<string>();
            foreach (DialogueScriptableObject dialogueSO in UngroupedDialogues)
            {
                if (startingDialogueFilter && dialogueSO.IsStartingDialogue)
                {
                    returnableNames.Add(dialogueSO.DialogueName);
                }
                else if (!startingDialogueFilter)
                {
                    returnableNames.Add(dialogueSO.DialogueName);
                }
            }
            return returnableNames;
        }
    }
}
