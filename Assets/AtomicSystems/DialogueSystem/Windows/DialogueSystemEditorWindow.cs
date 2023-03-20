using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DUJAL.Systems.Utils;
using DUJAL.Systems.Dialogue.Utils;
using DUJAL.Systems.Dialogue.Constants;
using System;

namespace DUJAL.Systems.Dialogue
{

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private Button _saveButton;
        private TextField _fileNameTF;
        private DialogueSystemGraphView graphView;

        [MenuItem(DialogueConstants.DialogueGraphWindowStream)]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>(DialogueConstants.DialogueGraphWindowTitle);
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolBar();
            AddStyles();
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            _fileNameTF = DialogueSystemUtils.CreateTextField(DialogueConstants.DefaultAssetFilename, DialogueConstants.FileNameLabel, callback => 
            {
                _fileNameTF.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            _saveButton = DialogueSystemUtils.CreateButon(DialogueConstants.SaveAssetButonText, () => Save());
            toolbar.Add(_fileNameTF);
            toolbar.Add(_saveButton);
            toolbar.AddStyleSheets($"{DialogueConstants.EditorDefaultResourcesPath}/{DialogueConstants.ToolbarStyleFilename}");
            rootVisualElement.Add(toolbar);
        }

        private void AddGraphView()
        {
            graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets(
                $"{DialogueConstants.EditorDefaultResourcesPath}/{DialogueConstants.VariablesFilename}"
            );
        }

        public void ToggleSaving(bool enabled) 
        {
            _saveButton.SetEnabled(enabled);
        }
        private void Save()
        {
            if (string.IsNullOrEmpty(_fileNameTF.value)) 
            {
                EditorUtility.DisplayDialog(
                    DialogueConstants.SaveInvalidFilenamePopupTitle,
                    DialogueConstants.SaveInvalidFilenamePopupText,
                    DialogueConstants.SaveInvalidFilenamePopupPrompt
                    );

                return;
            }
            DialogueSystemIO.Initialize(graphView, _fileNameTF.value);
            DialogueSystemIO.Save();
        }

    }
}