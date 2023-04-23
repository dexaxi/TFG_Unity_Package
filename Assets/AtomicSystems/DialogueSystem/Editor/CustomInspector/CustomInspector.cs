namespace DUJAL.Systems.Dialogue 
{
    using System.Collections.Generic;
    using UnityEditor;
    using DUJAL.Systems.Dialogue.Constants;
    using DUJAL.Systems.Dialogue.Utils;
    [CustomEditor(typeof(InspectorDialogue))]
    public class CustomInspector : Editor
    {
        //SOs
        private SerializedProperty _dialogueContainerSOProperty;
        private SerializedProperty _groupSOProperty;
        private SerializedProperty _dialogueSOProperty;
        private SerializedProperty _currentPlayedDialogueProperty;
        //Filters
        private SerializedProperty _startingDialogueFilterProperty;
        private SerializedProperty _groupedDialogueFilterProperty;
        //Index
        private SerializedProperty _selectedGroupProperty;
        private SerializedProperty _selectedDialogueProperty;

        //UI Data
        private SerializedProperty _textProperty;
        private SerializedProperty _speakerImageProperty;
        private SerializedProperty _audioStyleProperty;
        private SerializedProperty _textSpeedProperty;
        private SerializedProperty _maxLineCountProperty;
        private SerializedProperty _autoTextProperty;
        private SerializedProperty _enterProperty;
        private SerializedProperty _exitProperty;
        private SerializedProperty _letterRevealedProperty;

        private void OnEnable()
        {
            _dialogueContainerSOProperty = serializedObject.FindProperty("_dialogueContainerSO");
            _groupSOProperty = serializedObject.FindProperty("_groupSO");
            _dialogueSOProperty = serializedObject.FindProperty("_dialogueSO");
            _currentPlayedDialogueProperty = serializedObject.FindProperty("_currentPlayedDialogue");
            _startingDialogueFilterProperty = serializedObject.FindProperty("_startingDialogueFilter");
            _groupedDialogueFilterProperty = serializedObject.FindProperty("_groupedDialogueFilter");
            
            _selectedGroupProperty = serializedObject.FindProperty("_selectedGroup");
            _selectedDialogueProperty = serializedObject.FindProperty("_selectedDialogue");

            _textProperty = serializedObject.FindProperty("_text");
            _speakerImageProperty = serializedObject.FindProperty("_speakerImage");
            _audioStyleProperty = serializedObject.FindProperty("_audioStyle");
            _textSpeedProperty = serializedObject.FindProperty("_textSpeed");
            _maxLineCountProperty = serializedObject.FindProperty("_maxLineCount");
            _autoTextProperty = serializedObject.FindProperty("_autoText");
            _enterProperty = serializedObject.FindProperty("Enter");
            _exitProperty = serializedObject.FindProperty("Exit");
            _letterRevealedProperty = serializedObject.FindProperty("LetterRevealed");
        }

        public override void OnInspectorGUI()
        {
            //Update SO
            serializedObject.Update();
            
            //Draw Container
            DrawContainerField();
            
            //We get the Dialogue Container Scriptable Object and check if null so we show the info in the editor 
            DialogueContainerScriptableObject containerSO = (DialogueContainerScriptableObject) _dialogueContainerSOProperty.objectReferenceValue;
            
            if (containerSO == null)
            {
                DrawHelp(DialogueConstants.CustomInspectorDialogueContainerHelpBox);
                return;
            }
           
            //Draw Filters
            DrawFiltersField();

            List<string> dialogueNames;
            string dialoguePath = $"{DialogueConstants.DialogueEditorGraphsPath}/{containerSO.FileName}";
            string dialogueMissingMessage;
            bool startingDialogueFilter = _startingDialogueFilterProperty.boolValue;
            //If !filter -> dont show groups. Also if no available groups show error.
            if (_groupedDialogueFilterProperty.boolValue)
            {
                List<string> groupNames = containerSO.GetDialogueGroupNames();
                if (groupNames.Count == 0)
                {
                    DrawHelp(DialogueConstants.CustomInspectorGroupHelpBox);
                    return;
                }

                DrawGroupField(containerSO, groupNames);
                GroupScriptableObject groupSO = (GroupScriptableObject)_groupSOProperty.objectReferenceValue;
                dialogueNames = containerSO.GetGroupedDialogueNames(groupSO, startingDialogueFilter);
                dialoguePath += $"/{DialogueConstants.GroupsContainerFolderName}/{groupSO.GroupName}";
                dialogueMissingMessage = DialogueConstants.CustomInspectorGroupedDialogueHelpBox1;
                dialogueMissingMessage += startingDialogueFilter ?
                    $" {DialogueConstants.CustomInspectorGroupedStartingDialogueHelpBox} {DialogueConstants.CustomInspectorGroupedDialogueHelpBox2}" :
                    $" {DialogueConstants.CustomInspectorGroupedDialogueHelpBox2}";
            }
            else 
            {
                dialogueNames = containerSO.GetUnGroupedDialogueNames(startingDialogueFilter);
                dialoguePath += $"/{DialogueConstants.GlobalContainerFolderName}";
                dialogueMissingMessage = DialogueConstants.CustomInspectorGroupedDialogueHelpBox1;
                dialogueMissingMessage += startingDialogueFilter ?
                    $" {DialogueConstants.CustomInspectorGroupedStartingDialogueHelpBox} {DialogueConstants.CustomInspectorUnGroupedDialogueHelpBox}" : 
                    $" {DialogueConstants.CustomInspectorUnGroupedDialogueHelpBox}";
            }

            if (dialogueNames.Count == 0) 
            {
                DrawHelp(dialogueMissingMessage);
                return;
            }


            DrawDialogueField(dialogueNames, dialoguePath);
            
            DrawUIField();

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawContainerField() 
        {
            DialogueSystemUtils.DrawHeader(DialogueConstants.CustomInspectorDialogueContainer);
            _dialogueContainerSOProperty.DrawPropertyField();
            DialogueSystemUtils.DrawSpace();
        }
        private void DrawFiltersField() 
        {
            DialogueSystemUtils.DrawHeader(DialogueConstants.CustomInspectorFilter);
            _startingDialogueFilterProperty.DrawPropertyField();
            _groupedDialogueFilterProperty.DrawPropertyField();
            DialogueSystemUtils.DrawSpace();
        }
        private void DrawGroupField(DialogueContainerScriptableObject containerSO, List<string> groupNames)
        {
            DialogueSystemUtils.DrawHeader(DialogueConstants.CustomInspectorGroup);

            int previousIndex = _selectedGroupProperty.intValue;
            GroupScriptableObject oldGroup = (GroupScriptableObject)_groupSOProperty.objectReferenceValue;
            bool isOldGroupNull = oldGroup == null;
            string oldGroupName = isOldGroupNull ? "" : oldGroup.GroupName;

            _selectedGroupProperty.UpdateIndexOnNameListChange(groupNames, previousIndex, oldGroupName, isOldGroupNull);

            _selectedGroupProperty.intValue = DialogueSystemUtils.DrawPopup(DialogueConstants.CustomInspectorGroup, _selectedGroupProperty, groupNames.ToArray());
           
            string selectedGroupName = groupNames[_selectedGroupProperty.intValue];

            GroupScriptableObject group = DialogueSystemIO.LoadAsset<GroupScriptableObject>(
                $"{DialogueConstants.DialogueEditorGraphsPath}/{containerSO.FileName}/{DialogueConstants.GroupsContainerFolderName}/{selectedGroupName}", selectedGroupName);

            _groupSOProperty.objectReferenceValue = group;

            DialogueSystemUtils.DrawDisabledFields(() => _groupSOProperty.DrawPropertyField());
            DialogueSystemUtils.DrawSpace();
        }

        private void DrawDialogueField(List<string> dialogueNames, string path) 
        {
            DialogueSystemUtils.DrawHeader(DialogueConstants.CustomInspectorDialogue);

            int previousIndex = _selectedDialogueProperty.intValue;
            DialogueScriptableObject oldDialogue = (DialogueScriptableObject)_dialogueSOProperty.objectReferenceValue;
            bool isOldDialogueNull = oldDialogue == null;
            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;
            
            _selectedDialogueProperty.UpdateIndexOnNameListChange(dialogueNames, previousIndex, oldDialogueName, isOldDialogueNull);
            
            _selectedDialogueProperty.intValue = DialogueSystemUtils.DrawPopup(DialogueConstants.CustomInspectorDialogue, _selectedDialogueProperty, dialogueNames.ToArray());

            string dialogueName = dialogueNames[_selectedDialogueProperty.intValue];

            DialogueScriptableObject dialogue = DialogueSystemIO.LoadAsset<DialogueScriptableObject>(path, dialogueName);

            _dialogueSOProperty.objectReferenceValue = dialogue;
            DialogueSystemUtils.DrawDisabledFields(() => _dialogueSOProperty.DrawPropertyField());
            _currentPlayedDialogueProperty.objectReferenceValue = DialogueSystemIO.LoadAsset<DialogueScriptableObject>(DialogueConstants.DialogueEditorGraphsPath, "Playable Dialogue");
            DialogueSystemUtils.DrawDisabledFields(() => _currentPlayedDialogueProperty.DrawPropertyField());

        }
        
        public void DrawHelp(string helpBoxText, MessageType type = MessageType.Info)
        {
            DialogueSystemUtils.DrawHelp(helpBoxText, type);
            DialogueSystemUtils.DrawSpace();
            DialogueSystemUtils.DrawHelp(DialogueConstants.CustomInspectorNoDialogueWarn, MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUIField()
        {
            DialogueSystemUtils.DrawHeader("UI");
            _textProperty.DrawPropertyField();
            _speakerImageProperty.DrawPropertyField();
            _audioStyleProperty.DrawPropertyField();
            _textSpeedProperty.DrawPropertyField();
            _maxLineCountProperty.DrawPropertyField();
            _autoTextProperty.DrawPropertyField();
            DialogueSystemUtils.DrawSpace();
            _enterProperty.DrawPropertyField();
            _letterRevealedProperty.DrawPropertyField();
            _exitProperty.DrawPropertyField();
            
            
        }

    }
}
