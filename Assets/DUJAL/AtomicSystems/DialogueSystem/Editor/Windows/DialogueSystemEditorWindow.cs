

namespace DUJAL.Systems.Dialogue
{
    using UnityEditor;
    using UnityEngine.UIElements;
    using UnityEditor.UIElements;
    using DUJAL.Systems.Utils;
    using DUJAL.Systems.Dialogue.Utils;
    using DUJAL.Systems.Dialogue.Constants;
    using System.IO;

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private Button _saveButton;
        private Button _minimapButton;
        private static TextField _fileNameTF;
        private DialogueSystemGraphView _graphView;

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
            Button LoadButton  = DialogueSystemUtils.CreateButon(DialogueConstants.LoadAssetButonText, () => Load());
            Button clearButton = DialogueSystemUtils.CreateButon(DialogueConstants.ClearAssetButonText, () => Clear());
            Button resetButton  = DialogueSystemUtils.CreateButon(DialogueConstants.ResetAssetButonText, () => ResetGraphView());
            _minimapButton = DialogueSystemUtils.CreateButon(DialogueConstants.MinimapButonText, () => ToggleMinimapView());
            toolbar.Add(_fileNameTF);
            toolbar.Add(_saveButton);
            toolbar.Add(LoadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(_minimapButton);
            toolbar.AddStyleSheets($"{DialogueConstants.EditorDefaultResourcesPath}/{DialogueConstants.ToolbarStyleFilename}");
            rootVisualElement.Add(toolbar);
        }


        private void AddGraphView()
        {
            _graphView = new DialogueSystemGraphView(this);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
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

        public static void UpdateFileName(string newFileName) 
        {
            _fileNameTF.value = newFileName;
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
            DialogueSystemIO.Initialize(_graphView, _fileNameTF.value);
            DialogueSystemIO.Save();
        }
        private void Load()
        {
            string loadPath = EditorUtility.OpenFilePanel(DialogueConstants.LoadGraphDialogueTitle, 
                $"{DialogueConstants.DialogueEditorGraphsPath}/{DialogueConstants.DialogueEditorGraphsFolder}", "asset");
            if (string.IsNullOrEmpty(loadPath))
            {
                return;
            }
            DialogueSystemIO.Initialize(_graphView, Path.GetFileNameWithoutExtension(loadPath));
            Clear();
            DialogueSystemIO.Load();
        }

        private void Clear() 
        {
            _graphView.ClearGraph();
        }

        private void ResetGraphView() 
        {
            _graphView.ClearGraph();
            UpdateFileName(DialogueConstants.DefaultAssetFilename);
        }

        private void ToggleMinimapView()
        {
            _graphView.ToggleMinimap();
            _minimapButton.ToggleInClassList(DialogueConstants.ToolbarButtonStyle);
        }

    }
}