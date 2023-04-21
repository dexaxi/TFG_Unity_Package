using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DUJAL.Systems.Dialogue.Constants 
{
    public static class DialogueConstants
    {
        public const string EditorDefaultResourcesPath = "Assets/AtomicSystems/DialogueSystem/EditorDefaultResources";
        public const string DialogueGraphWindowStream = "DUJAL/Dialogue Graph";
        
        public const string DialogueGraphWindowTitle = "Dialogue Graph";
        public const string FileNameLabel = "File Name:";
        public const string DefaultAssetFilename = "NewDialogueGraph";
        public const string SaveAssetButonText = "Save";
        public const string LoadAssetButonText = "Load";
        public const string ClearAssetButonText = "Clear";
        public const string ResetAssetButonText = "Reset";
        
        
        public const string AddSingleChoiceText = "Add Single Choice Node";
        public const string AddMultiplechoiceText = "Add Multiple Choice Node";
        public const string AddGroupContextMenuText = "Add Group";
        public const string NewGroupDefaultName = "DialogueGroup";
        public const string DisconnectInputText = "Disconnect Input Ports";
        public const string DisconnectOutputText = "Disconnect Output Ports";
        
        public const string NodeFirstChoicDefaultText = "Next Dialogue";
        
        public const string MultipleChoiceAddChoicDefaultText= "Add Choice";
        public const string MultipleChoiceNewChoicDefaultText= "New choice";
        public const string MultipleChoiceXText = "X";
        
        public const string BaseNodeDefaultNodeName = "DialogueName";
        
        public const string BaseNodeQuoteTitle = "Dialogue Text";
        public const string BaseNodeDialogueConnectionDefaultValue = "Dialogue Connection";
        public const string BaseNodeVoiceLinetitle = "Voice Line";
        public const string BaseNodeImageField = "Speaker Image";
        public const string BaseNodeColorField = "Dialogue Color";
        public const string BaseNodeQuoteDefaultValue = "Dialogue Text";

        public const string SearchWindowCreateElementText = "Create Element";
        public const string SearchWindowDialogueNodeText = "Dialogue Node";
        public const string SearchWindowSinglechoiceText = "Single Choice";
        public const string SearchWindowMultipleChoiceText = "Multiple Choice";
        public const string SearchWindowDialogueGroupText = "Dialogue Group";
        public const string SearchWindowDialogueSingleGroupText = "Single Group";

        
        public const string VariablesFilename = "Variables.uss";
        public const string ToolbarStyleFilename = "ToolbarStyle.uss";
        public const string GraphViewStyleFilename = "GraphViewStyle.uss";
        public const string NodeStyleFilename = "NodeStyle.uss";
        public const string ButtonStyleSheet = "ds-n-b";
        public const string TextFieldStyleSheet= "ds-n-tf";
        public const string TextFieldHiddenStyleSheet= "ds-n-tf-hidden";
        public const string TextFieldFilenameStyleSheet= "ds-n-filename-tf";
        public const string TextFieldChoiceStyleSheet= "ds-n-choice-tf";
        public const string TextFieldQuoteStyleSheet= "ds-n-quote-tf";
        public const string DataContainerStyleSheet= "ds-n-data-cont";
        public const string MainContainerStyleSheet= "ds-n-main-cont";
        public const string ExternalContainerStyleSheet= "ds-n-ext-cont";
        public const string ObjectFieldStyleSheet = "ds-n-of";
        public const string ColorFieldStyleSheet = "ds-n-cf";

        public static Color DefaultNodeBackgroundColor = new Color(29f/255f, 29f / 255f, 30f / 255f);

        public const string DialogueEditorGraphsPath = "Assets/AtomicSystems/DialogueSystem/SavedDialogues";
        public const string DialogueEditorGraphsFolder = "Graphs";

        public const string AssetsFolderName = "Assets/AtomicSystems/DialogueSystem";
        public const string DialogueSystemDataContainerFolderName = "SavedDialogues";
        public const string GlobalContainerFolderName = "Global";
        public const string GroupsContainerFolderName = "Groups";

        public const string SaveInvalidFilenamePopupTitle = "Invalid File Name";
        public const string SaveInvalidFilenamePopupText = "Please ensure the filename is valid.";
        public const string SaveInvalidFilenamePopupPrompt = "Ok";
        
        public const string LoadInvalidFilenamePopupTitle = "Could not load the file";
        public const string LoadInvalidFilenamePopupText = "The file at the following path was not found.";
        public const string LoadInvalidFilenamePopupText2 = "Please make sure that this file is a valid Dialogue Graph.";
        public const string LoadInvalidFilenamePopupPrompt = "Ok";

    }
}
